using System.Runtime.InteropServices;
using Backdash.Serialization;
using Backdash.Serialization.Internal;

namespace Backdash.Network.Messages;

[Serializable, StructLayout(LayoutKind.Sequential, Pack = 4)]
record struct QualityReport : IUtf8SpanFormattable
{
    public int FrameAdvantage; /* what's the other guy's frame advantage? */
    public long Ping;

    public readonly void Serialize(in BinarySpanWriter writer)
    {
        writer.Write(in FrameAdvantage);
        writer.Write(in Ping);
    }

    public void Deserialize(in BinaryBufferReader reader)
    {
        FrameAdvantage = reader.ReadInt32();
        Ping = reader.ReadInt64();
    }

    public readonly bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        bytesWritten = 0;
        using Utf8ObjectStringWriter writer = new(in utf8Destination, ref bytesWritten);
        return writer.Write(in FrameAdvantage) && writer.Write(in Ping);
    }
}
