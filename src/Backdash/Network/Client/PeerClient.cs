using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Backdash.Core;
using Backdash.Serialization;

namespace Backdash.Network.Client;

/// <summary>
///     Message sent handler.
/// </summary>
public interface IMessageHandler<T> where T : struct
{
    /// <summary>
    ///     Handle messages sent.
    /// </summary>
    void AfterSendMessage(int bytesSent);

    /// <summary>
    ///     Prepare messages to be sent.
    /// </summary>
    void BeforeSendMessage(ref T message);
}

sealed class PeerClient<T> : INetcodeJob, IDisposable, IAsyncDisposable where T : struct
{
    readonly IPeerObserver<T> observer;
    readonly IBinarySerializer<T> serializer;
    readonly Logger logger;
    readonly LatencyWaiter? latencyWaiter;
    readonly int maxPacketSize;
    readonly int receiveSocketAddressSize;
    readonly Channel<QueueEntry> sendQueue;
    CancellationTokenSource? cancellation;
    public string JobName { get; }

    public IPeerSocket Socket { get; }

    struct QueueEntry(T body, SocketAddress recipient, long queuedAt, IMessageHandler<T>? callback)
    {
        public T Body = body;
        public readonly SocketAddress Recipient = recipient;
        public readonly long QueuedAt = queuedAt;
        public readonly IMessageHandler<T>? Callback = callback;
    }

    public PeerClient(
        IPeerSocket socket,
        IBinarySerializer<T> serializer,
        IPeerObserver<T> observer,
        Logger logger,
        LatencyWaiter? latencyWaiter = null,
        int maxPacketSize = Max.UdpPacketSize,
        int maxPackageQueue = Max.PackageQueue,
        int receiveSocketAddressSize = 0
    )
    {
        ArgumentNullException.ThrowIfNull(socket);
        ArgumentNullException.ThrowIfNull(observer);
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(logger);

        Socket = socket;
        this.observer = observer;
        this.serializer = serializer;
        this.logger = logger;
        this.latencyWaiter = latencyWaiter;
        this.maxPacketSize = maxPacketSize;

        this.receiveSocketAddressSize =
            receiveSocketAddressSize > 0
                ? receiveSocketAddressSize
                : SocketAddress.GetMaximumAddressSize(socket.AddressFamily);

        sendQueue = Channel.CreateBounded<QueueEntry>(
            new BoundedChannelOptions(maxPackageQueue)
            {
                SingleWriter = false,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropOldest,
            });

        JobName = $"{nameof(UdpClient)} ({socket.Port})";
    }

    public Task Start(CancellationToken cancellationToken) => ProcessMessages(cancellationToken);

    public async Task ProcessMessages(CancellationToken cancellationToken)
    {
        if (cancellation is not null)
            return;

        cancellation = new();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cancellation.Token);
        var token = cts.Token;

        await Task.WhenAll(StartReceiving(token), StartSending(token)).ConfigureAwait(false);
    }

    public int BindPort => Socket.Port;

    async Task StartSending(CancellationToken cancellationToken)
    {
        var buffer = Mem.AllocatePinnedMemory(maxPacketSize);
        var reader = sendQueue.Reader;
        cancellationToken.Register(() => sendQueue.Writer.TryComplete());

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (!await reader.WaitToReadAsync(CancellationToken.None).ConfigureAwait(false)
                    || cancellationToken.IsCancellationRequested)
                    break;

                while (reader.TryRead(out var entry))
                {
                    latencyWaiter?.Wait(entry.QueuedAt);
                    entry.Callback?.BeforeSendMessage(ref entry.Body);

                    // LATER: move the .Span outside the loop (requires >=.NET10)
                    var bodySize = serializer.Serialize(in entry.Body, buffer.Span);
                    var sentSize = await Socket.SendToAsync(buffer[..bodySize], entry.Recipient, cancellationToken)
                        .ConfigureAwait(false);

                    ThrowIf.Assert(sentSize == bodySize);

                    entry.Callback?.AfterSendMessage(sentSize);
                }
            }
            catch (Exception ex)
                when (ex is TaskCanceledException or OperationCanceledException or ChannelClosedException)
            {
                break;
            }
            catch (SocketException ex)
            {
                if (logger.EnabledLevel is not LogLevel.None)
                    logger.Write(LogLevel.Error, $"Socket send error: {ex}");
                break;
            }
            catch (Exception ex)
            {
                if (logger.EnabledLevel is not LogLevel.None)
                    logger.Write(LogLevel.Error, $"Socket send error: {ex}");
                break;
            }
        }
    }

    async Task StartReceiving(CancellationToken cancellationToken)
    {
        var buffer = Mem.AllocatePinnedArray(maxPacketSize);
        SocketAddress address = new(Socket.AddressFamily, receiveSocketAddressSize);
        T msg = default;
        while (!cancellationToken.IsCancellationRequested)
        {
            int receivedSize;
            try
            {
                receivedSize = await Socket
                    .ReceiveFromAsync(buffer, address, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
            {
                break;
            }
            catch (SocketException ex)
            {
                if (logger.EnabledLevel is not LogLevel.None)
                    logger.Write(LogLevel.Error, $"Socket rcv error: {ex}");
                break;
            }
            catch (Exception ex)
            {
                if (logger.EnabledLevel is not LogLevel.None)
                    logger.Write(LogLevel.Error, $"Socket rcv error: {ex}");
                break;
            }

            if (receivedSize is 0)
                continue;

            try
            {
                serializer.Deserialize(buffer.AsSpan(..receivedSize), ref msg);
                observer.OnPeerMessage(in msg, address, receivedSize);
            }
            catch (NetcodeDeserializationException ex)
            {
                logger.Write(LogLevel.Warning, $"UDP Message error: {ex}");
            }
        }

        // ReSharper disable once RedundantAssignment
#pragma warning disable S1854
        buffer = null;
#pragma warning restore S1854
    }

    public ValueTask SendTo(SocketAddress peerAddress, in T payload,
        IMessageHandler<T>? callback = null, CancellationToken cancellationToken = default) =>
        sendQueue.Writer.WriteAsync(new(payload, peerAddress, Stopwatch.GetTimestamp(), callback), cancellationToken);

    public bool TrySendTo(SocketAddress peerAddress, in T payload, IMessageHandler<T>? callback = null) =>
        sendQueue.Writer.TryWrite(new(payload, peerAddress, Stopwatch.GetTimestamp(), callback));

    public async ValueTask StopAsync()
    {
        sendQueue.Writer.TryComplete();

        if (cancellation is { IsCancellationRequested: false })
            await cancellation.CancelAsync();

        Socket.Close();
    }

    public void Stop()
    {
        sendQueue.Writer.TryComplete();

        if (cancellation is { IsCancellationRequested: false })
            cancellation.Cancel();

        Socket.Close();
    }

    public void Dispose()
    {
        Stop();
        DisposeInternal();
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        DisposeInternal();
    }

    void DisposeInternal()
    {
        cancellation?.Dispose();
        Socket.Dispose();
    }
}
