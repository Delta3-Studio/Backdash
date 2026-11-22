using System.Runtime.CompilerServices;
using Backdash.Network.Messages;
using Backdash.Serialization.Internal;

namespace Backdash.Network.Protocol.Comm;

static class InputEncoder
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DeltaXorRle.Encoder GetCompressor(ref InputMessage inputMsg, Span<byte> lastBuffer) =>
        new(inputMsg.Bits, lastBuffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DeltaXorRle.Decoder GetDecompressor(ref InputMessage inputMsg) =>
        new(inputMsg.Bits, inputMsg.NumBits);
}
