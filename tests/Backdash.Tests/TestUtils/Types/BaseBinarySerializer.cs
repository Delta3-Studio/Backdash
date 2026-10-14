using Backdash.Network;
using Backdash.Serialization;
using Backdash.Serialization.Internal;

namespace Backdash.Tests.TestUtils.Types;

/// <inheritdoc />
abstract class BaseBinarySerializer<T>(Endianness endianness = Endianness.BigEndian) : IBinarySerializer<T>
{
    readonly EndiannessSerializer.INumberSerializer numberSerializer = EndiannessSerializer.Get(endianness);
    public Endianness Endianness => endianness;

    /// <summary>
    /// Serialize <paramref name="data"/> using <see cref="BinarySpanWriter"/>
    /// </summary>
    /// <param name="writer">Binary writer</param>
    /// <param name="data">Data to be written</param>
    protected abstract void Serialize(in BinarySpanWriter writer, in T data);

    /// <summary>
    /// Deserialize buffer data using <paramref name="reader"/> into <paramref name="result"/>
    /// </summary>
    /// <param name="reader">Binary reader</param>
    /// <param name="result">Reference to be set with the deserialized value.</param>
    protected abstract void Deserialize(in BinaryBufferReader reader, ref T result);

    int IBinarySerializer<T>.Serialize(in T data, Span<byte> buffer)
    {
        var offset = 0;
        BinarySpanWriter writer = new(buffer, ref offset, numberSerializer);
        Serialize(in writer, in data);
        return writer.WrittenCount;
    }

    int IBinarySerializer<T>.Deserialize(ReadOnlySpan<byte> data, ref T value)
    {
        var offset = 0;
        BinaryBufferReader reader = new(data, ref offset, numberSerializer);
        Deserialize(in reader, ref value);
        return offset;
    }
}
