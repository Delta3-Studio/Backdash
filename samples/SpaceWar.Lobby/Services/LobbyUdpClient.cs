using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using Backdash.Network.Client;
using SpaceWar.Models;

namespace SpaceWar.Services;

public sealed class LobbyUdpClient : IDisposable
{
    readonly IPEndPoint serverEndpoint;
    readonly UdpSocket socket;
    readonly CancellationTokenSource cts = new();
    readonly byte[] sendBuffer = GC.AllocateArray<byte>(Unsafe.SizeOf<Guid>(), pinned: true);
    readonly HashSet<Guid> knownClients = [];
    bool disposed;

    public LobbyUdpClient(int localPort, Uri serverUrl, int serverPort)
    {
        var serverAddress = UdpSocket.GetDnsIpAddress(serverUrl.DnsSafeHost);
        serverEndpoint = new(serverAddress, serverPort);
        socket = new(localPort);

        Task.Run(() => Receive(cts.Token));
    }

    public async Task HandShake(User user, CancellationToken ct = default)
    {
        if (!user.Token.TryWriteBytes(sendBuffer, true, out var bytesWritten) || bytesWritten is 0)
            return;

        await socket.SendToAsync(sendBuffer.AsMemory()[..bytesWritten], serverEndpoint, ct);
    }

    public async Task Ping(User user, Peer[] peers, CancellationToken ct = default)
    {
        if (peers.Length is 0 || !user.PeerId.TryWriteBytes(sendBuffer, true, out var bytesWritten) ||
            bytesWritten is 0)
            return;

        var msgBytes = sendBuffer.AsMemory()[..bytesWritten];
        for (var i = 0; i < peers.Length; i++)
        {
            var peer = peers[i];
            if (peer.Connected && peer.PeerId != user.PeerId)
                await socket.SendToAsync(msgBytes, peer.GetEndpointForUser(user), ct);
        }
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    async ValueTask Receive(CancellationToken stoppingToken)
    {
        var idSize = Unsafe.SizeOf<Guid>();
        var recBuffer = GC.AllocateArray<byte>(idSize, pinned: true);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiveInfo = await socket
                    .ReceiveAsync(recBuffer, stoppingToken)
                    .ConfigureAwait(false);

                if (receiveInfo.ReceivedBytes is 0) continue;

                if (receiveInfo.ReceivedBytes != idSize)
                {
                    var msg = Encoding.UTF8.GetString(recBuffer.AsSpan(0, receiveInfo.ReceivedBytes));
                    Console.WriteLine($"recv ({receiveInfo.RemoteEndPoint}): '{msg}'");
                    continue;
                }

                Guid peerToken = new(recBuffer.AsSpan(0, receiveInfo.ReceivedBytes), true);
                Console.WriteLine($"recv ({receiveInfo.RemoteEndPoint}): Ping from '{peerToken}'");
                if (peerToken != Guid.Empty) knownClients.Add(peerToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // skip
            }
        }
    }

    public bool IsKnown(Guid id) => knownClients.Contains(id);

    public void Stop()
    {
        if (!cts.IsCancellationRequested)
            cts.Cancel();

        socket.Close();
    }

    public void Dispose()
    {
        Stop();
        if (disposed) return;
        disposed = true;
        socket.Dispose();
        cts.Dispose();
    }
}
