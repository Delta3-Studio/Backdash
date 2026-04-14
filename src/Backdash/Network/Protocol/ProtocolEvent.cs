using Backdash.Core;

namespace Backdash.Network.Protocol;

struct ProtocolEventInfo(NetcodePlayer player, PeerEventInfo eventInfo) : IUtf8SpanFormattable
{
    public readonly NetcodePlayer Player = player;
    public PeerEventInfo EventInfo = eventInfo;
    public readonly PeerEvent Type => EventInfo.Type;

    public readonly bool TryFormat(
        Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        bytesWritten = 0;
        Utf8StringBuilder writer = new(in utf8Destination, ref bytesWritten);
        if (!writer.Write("P"u8)) return false;
        if (!writer.Write(Player.Index)) return false;
        if (!writer.Write(" Info: "u8)) return false;
        if (!writer.Write(EventInfo)) return false;
        return true;
    }
}
