using Backdash.Serialization;
using Backdash.Serialization.Internal;

namespace Backdash.Network.Messages;

[Serializable]
record struct QualityReply : IUtf8SpanFormattable
{
    public long Pong;

    public readonly void Serialize(in BinarySpanWriter writer) =>
        writer.Write(in Pong);

    public void Deserialize(in BinaryBufferReader reader) =>
        Pong = reader.ReadInt64();

    public readonly bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        bytesWritten = 0;
        using Utf8ObjectStringWriter writer = new(in utf8Destination, ref bytesWritten);
        return writer.Write(in Pong);
    }
}
