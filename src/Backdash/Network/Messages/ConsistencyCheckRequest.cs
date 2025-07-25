using Backdash.Serialization;
using Backdash.Serialization.Internal;

namespace Backdash.Network.Messages;

[Serializable]
record struct ConsistencyCheckRequest : IUtf8SpanFormattable
{
    public Frame Frame;

    public readonly void Serialize(in BinarySpanWriter writer) =>
        writer.Write(in Frame);

    public void Deserialize(in BinaryBufferReader reader) =>
        Frame = reader.ReadFrame();

    public readonly bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        bytesWritten = 0;
        using Utf8ObjectWriter writer = new(in utf8Destination, ref bytesWritten);
        return writer.Write(in Frame);
    }
}
