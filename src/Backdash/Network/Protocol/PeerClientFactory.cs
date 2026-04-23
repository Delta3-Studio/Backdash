using Backdash.Core;
using Backdash.Network.Client;
using Backdash.Network.Messages;
using Backdash.Network.Protocol.Comm;
using Backdash.Options;

namespace Backdash.Network.Protocol;

sealed class PeerClientFactory(
    NetcodeOptions options,
    IPeerSocketFactory socketFactory,
    Logger logger,
    ILatencyStrategy latencyStrategy
)
{
    public PeerClient<ProtocolMessage> CreateClient(int port, IPeerObserver<ProtocolMessage> observer) =>
        new(
            socketFactory.Create(port, options),
            new ProtocolMessageSerializer(options.Protocol.SerializationEndianness),
            observer,
            logger,
            latencyStrategy,
            options.Protocol.UdpPacketBufferSize,
            options.Protocol.MaxPackageQueue,
            options.Protocol.ReceiveSocketAddressSize
        )
        {
            NetworkLatency = options.Protocol.NetworkLatency,
        };
}
