using System.Runtime.CompilerServices;
using Backdash.Network;
using Backdash.Serialization;
using Backdash.Tests.TestUtils;
using Backdash.Tests.TestUtils.Types;

namespace Backdash.Tests.Specs.Unit.Serialization;

public class BinarySpanReadWriteSpanTests
{
    [PropertyTest]
    public bool SpanOfShort(short[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<short> read = stackalloc short[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfUShort(ushort[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<ushort> read = stackalloc ushort[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfInt(int[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<int> read = stackalloc int[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfUInt(uint[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<uint> read = stackalloc uint[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfLong(long[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<long> read = stackalloc long[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfULong(ulong[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<ulong> read = stackalloc ulong[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfInt128(Int128[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<Int128> read = stackalloc Int128[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfUInt128(UInt128[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<UInt128> read = stackalloc UInt128[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfByte(byte[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<byte> read = stackalloc byte[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfSByte(sbyte[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<sbyte> read = stackalloc sbyte[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfChars(char[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<char> read = stackalloc char[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOBooleans(bool[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<bool> read = stackalloc bool[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOFrame(Frame[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<Frame> read = stackalloc Frame[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOFrameSpan(FrameSpan[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<FrameSpan> read = stackalloc FrameSpan[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOFrameRange(FrameRange[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<FrameRange> read = stackalloc FrameRange[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOChecksum(Checksum[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        Span<Checksum> read = stackalloc Checksum[value.Length];
        reader.Read(read);
        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfUtf8(NonEmptyString input, Endianness endianness)
    {
        var value = input.Item;
        var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(value);

        var size = Setup(utf8Bytes, endianness, out var writer, out var reader);
        writer.WriteUtf8String(value);
        writer.WrittenCount.Should().Be(size);

        Span<char> read = stackalloc char[value.Length];
        reader.ReadUtf8String(read);

        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfUtf8Bytes(NonEmptyString input, Endianness endianness)
    {
        var value = input.Item;
        var utf8Size = System.Text.Encoding.UTF8.GetByteCount(value);
        var utf8Bytes = System.Text.Encoding.UTF8.GetBytes(value);

        var size = Setup(utf8Bytes, endianness, out var writer, out var reader);
        writer.WriteUtf8String(value);
        writer.WrittenCount.Should().Be(size);

        Span<byte> read = stackalloc byte[utf8Size];
        reader.Read(read);
        reader.ReadCount.Should().Be(utf8Size);
        return utf8Bytes.AsSpan().SequenceEqual(read);
    }

    [PropertyTest]
    public bool SpanOfUnmanagedStruct(SimpleStructData[] value, Endianness endianness)
    {
        var size = Setup(value, endianness, out var writer, out var reader);
        writer.WriteStruct(in value);
        writer.WrittenCount.Should().Be(size);

        Span<SimpleStructData> read = stackalloc SimpleStructData[value.Length];
        reader.ReadStruct(in read);

        reader.ReadCount.Should().Be(size);
        return value.AsSpan().SequenceEqual(read);
    }

    [Collection(SerialCollectionDefinition.Name)]
    public class EnumTests
    {
        static bool TestEnum<T>(T[] value, Endianness endianness) where T : unmanaged, Enum
        {
            var size = Setup(value, endianness, out var writer, out var reader);
            writer.WriteEnum(value);
            writer.WrittenCount.Should().Be(size);
            Span<T> read = stackalloc T[value.Length];
            reader.ReadEnum(read);
            reader.ReadCount.Should().Be(size);
            return value.AsSpan().SequenceEqual(read);
        }

        [PropertyTest]
        public bool SpanOfByte(ByteEnum[] value, Endianness endianness) => TestEnum(value, endianness);

        [PropertyTest]
        public bool SpanOfSByte(SByteEnum[] value, Endianness endianness) => TestEnum(value, endianness);

        [PropertyTest]
        public bool SpanOfInt16(Int16Enum[] value, Endianness endianness) => TestEnum(value, endianness);

        [PropertyTest]
        public bool SpanOfUInt16(UInt16Enum[] value, Endianness endianness) => TestEnum(value, endianness);

        [PropertyTest]
        public bool SpanOfInt32(Int32Enum[] value, Endianness endianness) => TestEnum(value, endianness);

        [PropertyTest]
        public bool SpanOfUInt32(UInt32Enum[] value, Endianness endianness) => TestEnum(value, endianness);

        [PropertyTest]
        public bool SpanOfInt64(Int64Enum[] value, Endianness endianness) => TestEnum(value, endianness);

        [PropertyTest]
        public bool SpanOfUInt64(UInt64Enum[] value, Endianness endianness) => TestEnum(value, endianness);
    }

    static int writeOffset;
    static int readOffset;

    static int Setup<T>(
        IReadOnlyCollection<T> values,
        Endianness endianness,
        out BinarySpanWriter writer,
        out BinaryBufferReader reader) where T : struct
    {
        var size = Unsafe.SizeOf<T>() * values.Count;
        Span<byte> buffer = new byte[size];
        writeOffset = 0;
        readOffset = 0;
        writer = new(buffer, ref writeOffset, endianness);
        reader = new(buffer, ref readOffset, endianness);
        return size;
    }
}
