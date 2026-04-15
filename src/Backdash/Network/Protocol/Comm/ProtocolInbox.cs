using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backdash.Core;
using Backdash.Data;
using Backdash.Network.Client;
using Backdash.Network.Messages;
using Backdash.Options;
using Backdash.Serialization;
using Backdash.Synchronizing.Input;
using Backdash.Synchronizing.State;

namespace Backdash.Network.Protocol.Comm;

interface IProtocolInbox<TInput> : IPeerObserver<ProtocolMessage> where TInput : unmanaged
{
    GameInput<TInput> LastReceivedInput { get; }
    Frame LastAckedFrame { get; }
}

sealed class ProtocolInbox<TInput>(
    ProtocolOptions options,
    IBinarySerializer<TInput> inputSerializer,
    ProtocolState state,
    ProtocolSynchronizer sync,
    IMessageSender messageSender,
    IProtocolNetworkEventHandler networkEvents,
    IProtocolInputEventPublisher<TInput> inputEvents,
    ChecksumStore checksumStore,
    Logger logger
) : IProtocolInbox<TInput> where TInput : unmanaged
{
    ushort nextReceivedSeq;
    GameInput<TInput> lastReceivedInput = new();
    readonly byte[] lastReceivedInputBuffer = Mem.AllocatePinnedArray(Max.CompressedBytes);

    public GameInput<TInput> LastReceivedInput => lastReceivedInput;
    public Frame LastAckedFrame { get; private set; } = Frame.Null;

    public void OnPeerMessage(ref readonly ProtocolMessage message, in SocketAddress from, int bytesReceived)
    {
        if (!from.Equals(state.PeerAddress.Address))
            return;

        if (message.Header.Type is MessageType.Unknown)
        {
            logger.Write(LogLevel.Warning, $"Invalid UDP protocol message received from {state.Player}.");
            return;
        }

        var seqNum = message.Header.SequenceNumber;
        if (message.Header.Type is not (MessageType.SyncRequest or MessageType.SyncReply))
        {
            if (state.CurrentStatus is not ProtocolStatus.Running)
            {
                logger.Write(LogLevel.Trace, $"recv skip (not ready): {message} on {state.Player}");
                return;
            }

            if (message.Header.SyncNumber != state.RemoteSyncNumber)
            {
                logger.Write(LogLevel.Debug, $"recv rejecting: {message} on {state.Player}");
                return;
            }

            var skipped = (ushort)(seqNum - nextReceivedSeq);
            if (skipped > options.MaxSequenceDistance)
            {
                logger.Write(LogLevel.Debug,
                    $"dropping out of order packet (seq: {seqNum}, last seq:{nextReceivedSeq})");
                return;
            }
        }

        nextReceivedSeq = seqNum;
        logger.Write(LogLevel.Trace, $"recv {message} from {state.Player}");
        if (HandleMessage(in message, out var replyMsg))
        {
            if (replyMsg.Header.Type is not MessageType.Unknown && !messageSender.SendMessage(in replyMsg))
                logger.Write(LogLevel.Warning, $"inbox response dropped (seq: {seqNum})");

            state.Stats.Received.LastTime = Stopwatch.GetTimestamp();
            state.Stats.Received.TotalPackets++;
            state.Stats.Received.TotalBytes += (ByteSize)bytesReceived;
            if (state.Connection.DisconnectNotifySent && state.CurrentStatus is ProtocolStatus.Running)
            {
                networkEvents.OnNetworkEvent(PeerEvent.ConnectionResumed, state.Player);
                state.Connection.DisconnectNotifySent = false;
            }
        }
    }

    bool HandleMessage(ref readonly ProtocolMessage message, out ProtocolMessage replyMsg)
    {
        replyMsg = new(MessageType.Unknown);
        var handled = message.Header.Type switch
        {
            MessageType.SyncRequest => OnSyncRequest(in message, ref replyMsg),
            MessageType.SyncReply => OnSyncReply(in message, ref replyMsg),
            MessageType.Input => OnInput(in message.Input),
            MessageType.QualityReport => OnQualityReport(in message, ref replyMsg),
            MessageType.QualityReply => OnQualityReply(in message),
            MessageType.InputAck => OnInputAck(in message),
            MessageType.ConsistencyCheckRequest => OnConsistencyCheckRequest(in message, ref replyMsg),
            MessageType.ConsistencyCheckReply => OnConsistencyCheckReply(in message, ref replyMsg),
            MessageType.ConsistencyCheckFail => OnConsistencyCheckFail(in message),
            MessageType.KeepAlive => true,
            MessageType.Unknown =>
                throw new NetcodeException($"Unknown UDP protocol message received: {message.Header.Type}"),
            _ => throw new NetcodeException($"Invalid UDP protocol message received: {message.Header.Type}"),
        };
        return handled;
    }

    bool OnInput(in InputMessage msg)
    {
        logger.Write(LogLevel.Trace, $"Acked Frame: {LastAckedFrame}");
        /*
         * If a disconnect is requested, go ahead and disconnect now.
         */
        var disconnectRequested = msg.DisconnectRequested;
        if (disconnectRequested)
        {
            if (state.CurrentStatus is not ProtocolStatus.Disconnected && !state.Connection.DisconnectEventSent)
            {
                logger.Write(LogLevel.Information, "Disconnecting endpoint on remote request");
                networkEvents.OnNetworkEvent(PeerEvent.Disconnected, state.Player);
                state.Connection.DisconnectEventSent = true;
            }
        }
        else
        {
            /*
             * Update the peer connection status if this peer is still considered to be part
             * of the network.
             */
            Span<ConnectStatus> localStatus = state.PeerConnectStatuses;
            ReadOnlySpan<ConnectStatus> remoteStatus = msg.PeerConnectStatus;
            var peerCount = Math.Min(msg.PeerCount, localStatus.Length);

            ref var currentLocalStatus = ref MemoryMarshal.GetReference(localStatus);
            ref var currentRemoteStatus = ref MemoryMarshal.GetReference(remoteStatus);
            ref var limitRemoteStatus = ref Unsafe.Add(ref currentRemoteStatus, peerCount);

            while (Unsafe.IsAddressLessThan(ref currentRemoteStatus, ref limitRemoteStatus))
            {
                ThrowIf.Assert(currentRemoteStatus.LastFrame >= currentLocalStatus.LastFrame);
                currentLocalStatus.Disconnected = currentLocalStatus.Disconnected || currentRemoteStatus.Disconnected;
                currentLocalStatus.LastFrame = Frame.Max(currentLocalStatus.LastFrame, currentRemoteStatus.LastFrame);
                currentRemoteStatus = ref Unsafe.Add(ref currentRemoteStatus, 1)!;
                currentLocalStatus = ref Unsafe.Add(ref currentLocalStatus, 1)!;
            }
        }

        /*
         * Decompress the input.
         */
        var startLastReceivedFrame = lastReceivedInput.Frame;
        var lastReceivedFrame = lastReceivedInput.Frame;
        if (msg.NumBits > 0)
        {
            if (lastReceivedFrame < 0)
                lastReceivedFrame = msg.StartFrame.Previous();
            var nextFrame = lastReceivedFrame.Next();
            var currentFrame = msg.StartFrame;
            var decompressor = InputEncoder.GetDecompressor(ref Unsafe.AsRef(in msg));
            if (currentFrame < nextFrame)
            {
                var framesAhead = nextFrame.Number - currentFrame.Number;
                logger.Write(LogLevel.Trace,
                    $"Skipping past {framesAhead} frames (current: {currentFrame}, last: {lastReceivedFrame}, next: {nextFrame})");
                if (decompressor.Skip(framesAhead))
                    currentFrame += framesAhead;
                else
                    // probably we already heave all inputs from this message
                    return true;
            }

            ThrowIf.Assert(currentFrame == nextFrame);
            var lastReceivedBuffer = lastReceivedInputBuffer.AsSpan(..msg.InputSize);
            while (decompressor.Read(lastReceivedBuffer))
            {
                inputSerializer.Deserialize(lastReceivedBuffer, ref lastReceivedInput.Data);
                ThrowIf.Assert(currentFrame == lastReceivedFrame.Next());
                lastReceivedFrame = currentFrame;
                lastReceivedInput.Frame = currentFrame;
                state.Stats.LastReceivedInputTime = Stopwatch.GetTimestamp();
                currentFrame++;
                logger.Write(LogLevel.Trace,
                    $"Received input: frame {lastReceivedInput.Frame}, sending to emulator queue {state.Player} (ack: {LastAckedFrame})");
                inputEvents.Publish(new(state.Player, lastReceivedInput));
            }

            LastAckedFrame = msg.AckFrame;
        }

        ThrowIf.Assert(lastReceivedInput.Frame >= startLastReceivedFrame);
        return true;
    }

    bool OnInputAck(ref readonly ProtocolMessage msg)
    {
        LastAckedFrame = msg.InputAck.AckFrame;
        return true;
    }

    bool OnQualityReply(ref readonly ProtocolMessage msg)
    {
        state.Stats.RoundTripTime = Stopwatch.GetElapsedTime(msg.QualityReply.Pong);
        return true;
    }

    bool OnQualityReport(ref readonly ProtocolMessage msg, ref ProtocolMessage replyMsg)
    {
        replyMsg.Header.Type = MessageType.QualityReply;
        replyMsg.QualityReply.Pong = msg.QualityReport.Ping;
        state.Fairness.RemoteFrameAdvantage = new(msg.QualityReport.FrameAdvantage);
        return true;
    }

    bool OnSyncReply(ref readonly ProtocolMessage msg, ref ProtocolMessage replyMsg)
    {
        var elapsed = Stopwatch.GetElapsedTime(msg.SyncReply.Pong);
        if (state.CurrentStatus is not ProtocolStatus.Syncing)
        {
            logger.Write(LogLevel.Trace, "Ignoring SyncReply while not syncing");
            return msg.Header.SyncNumber == state.RemoteSyncNumber;
        }

        if (msg.SyncReply.RandomReply != state.Sync.CurrentRandom)
        {
            logger.Write(LogLevel.Debug,
                $"Sync reply {msg.SyncReply.RandomReply} != {state.Sync.CurrentRandom}. Keep looking.");
            return false;
        }

        if (!state.Connection.IsConnected)
        {
            networkEvents.OnNetworkEvent(PeerEvent.Connected, state.Player);
            state.Connection.IsConnected = true;
        }

        logger.Write(LogLevel.Debug,
            $"Checking sync state ({state.Sync.RemainingRoundTrips} round trips remaining)");
        if (options.NumberOfSyncRoundTrips >= state.Sync.RemainingRoundTrips)
            state.Sync.TotalRoundTripsPing = TimeSpan.Zero;
        state.Sync.TotalRoundTripsPing += elapsed;
        if (--state.Sync.RemainingRoundTrips is 0)
        {
            var ping = state.Sync.TotalRoundTripsPing / options.NumberOfSyncRoundTrips;
            logger.Write(LogLevel.Information,
                $"Player {state.Player.Index} Synchronized! (Ping: {ping.TotalMilliseconds:f4})");
            state.CurrentStatus = ProtocolStatus.Running;
            state.Stats.RoundTripTime = ping;
            lastReceivedInput.ResetFrame();
            state.RemoteSyncNumber = msg.Header.SyncNumber;
            networkEvents.OnNetworkEvent(state.Player, new(PeerEvent.Synchronized)
            {
                Synchronized = new(ping),
            });
        }
        else
        {
            networkEvents.OnNetworkEvent(state.Player, new(PeerEvent.Synchronizing)
                {
                    Synchronizing = new(
                        TotalSteps: options.NumberOfSyncRoundTrips,
                        CurrentStep: options.NumberOfSyncRoundTrips - state.Sync.RemainingRoundTrips
                    ),
                }
            );
            sync.CreateRequestMessage(ref replyMsg);
        }

        return true;
    }

    public bool OnSyncRequest(ref readonly ProtocolMessage msg, ref ProtocolMessage replyMsg)
    {
        var remoteMagicNumber = state.RemoteSyncNumber;
        if (remoteMagicNumber is not 0 && msg.Header.SyncNumber != remoteMagicNumber)
        {
            logger.Write(LogLevel.Warning,
                $"Ignoring sync request from unknown endpoint ({msg.Header.SyncNumber} != {remoteMagicNumber})");
            return false;
        }

        sync.CreateReplyMessage(in msg.SyncRequest, ref replyMsg);
        return true;
    }

    bool OnConsistencyCheckRequest(ref readonly ProtocolMessage message, ref ProtocolMessage replyMsg)
    {
        var checkFrame = message.ConsistencyCheckRequest.Frame;
        var checksum = checksumStore.Get(checkFrame);

        logger.Write(LogLevel.Debug, $"Received consistency request check for: {checkFrame} (reply {checksum})");

        if (checksum.IsEmpty)
        {
            logger.Write(LogLevel.Warning, $"Unable to find requested local checksum for {checkFrame}");
            return false;
        }

        replyMsg.Header.Type = MessageType.ConsistencyCheckReply;
        replyMsg.ConsistencyCheckReply.Frame = checkFrame;
        replyMsg.ConsistencyCheckReply.Checksum = checksum;
        return true;
    }

    bool OnConsistencyCheckReply(ref readonly ProtocolMessage message, ref ProtocolMessage replyMsg)
    {
        var checkFrame = message.ConsistencyCheckReply.Frame;
        var checksum = message.ConsistencyCheckReply.Checksum;
        var localChecksum = state.Consistency.AskedChecksum;

        logger.Write(LogLevel.Debug, $"Reply consistency-check for {checkFrame} #{checksum}");

        if (state.Consistency.AskedFrame != checkFrame || localChecksum.IsEmpty || checksum.IsEmpty)
        {
            logger.Write(LogLevel.Warning, $"Unable to find reply local checksum #{checksum} for {checkFrame}");
            return false;
        }

        if (localChecksum != checksum)
        {
            logger.Write(LogLevel.Error,
                $"Invalid remote checksum on frame {checkFrame}, {localChecksum} != {checksum}");

            replyMsg.Header.Type = MessageType.ConsistencyCheckFail;
            replyMsg.ConsistencyCheckFail.Frame = checkFrame;
            replyMsg.ConsistencyCheckFail.RemoteChecksum = checksum;
            replyMsg.ConsistencyCheckFail.LocalChecksum = localChecksum;

            networkEvents.OnNetworkEvent(state.Player, new(PeerEvent.ChecksumMismatch)
                {
                    ChecksumMismatch = new(
                        MismatchFrame: checkFrame,
                        LocalChecksum: localChecksum,
                        RemoteChecksum: checksum
                    ),
                }
            );

            return true;
        }

        logger.Write(LogLevel.Debug, $"Finish consistency-check request check for {checkFrame} #{checksum}");
        state.Consistency.LastCheck = Stopwatch.GetTimestamp();
        state.Consistency.AskedFrame = Frame.Null;
        state.Consistency.AskedChecksum = Checksum.Empty;
        return true;
    }

    bool OnConsistencyCheckFail(ref readonly ProtocolMessage message)
    {
        var body = message.ConsistencyCheckFail;
        logger.Write(LogLevel.Warning, $"Failure in consistency-check for {body}");

        networkEvents.OnNetworkEvent(state.Player, new(PeerEvent.ChecksumMismatch)
            {
                ChecksumMismatch = new(
                    MismatchFrame: body.Frame,
                    LocalChecksum: body.LocalChecksum,
                    RemoteChecksum: body.RemoteChecksum
                ),
            }
        );

        return true;
    }
}
