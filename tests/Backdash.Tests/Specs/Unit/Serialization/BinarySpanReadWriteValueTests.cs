using System.Numerics;
using System.Runtime.CompilerServices;
using Backdash.Network;
using Backdash.Serialization;
using Backdash.Serialization.Numerics;
using Backdash.Tests.TestUtils;
using Backdash.Tests.TestUtils.Types;

// ReSharper disable CompareOfFloatsByEqualityOperator
#pragma warning disable S1244

namespace Backdash.Tests.Specs.Unit.Serialization;

[Collection(SerialCollectionDefinition.Name)]
public class BinarySpanReadWriteValueTests
{
    [PropertyTest]
    public bool TestByte(byte value, Endianness endianness)
    {
        var size = Setup<byte>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadByte();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestSByte(sbyte value, Endianness endianness)
    {
        var size = Setup<sbyte>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadSByte();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestBool(bool value, Endianness endianness)
    {
        var size = Setup<bool>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadBoolean();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestChar(char value, Endianness endianness)
    {
        var size = Setup<char>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadChar();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestShort(short value, Endianness endianness)
    {
        var size = Setup<short>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadInt16();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestUShort(ushort value, Endianness endianness)
    {
        var size = Setup<ushort>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadUInt16();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestInt(int value, Endianness endianness)
    {
        var size = Setup<int>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadInt32();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestUInt(uint value, Endianness endianness)
    {
        var size = Setup<uint>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadUInt32();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestLong(long value, Endianness endianness)
    {
        var size = Setup<long>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadInt64();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestULong(ulong value, Endianness endianness)
    {
        var size = Setup<ulong>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadUInt64();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestInt128(Int128 value, Endianness endianness)
    {
        var size = Setup<Int128>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadInt128();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestIntU128(UInt128 value, Endianness endianness)
    {
        var size = Setup<UInt128>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadUInt128();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestHalf(Half value, Endianness endianness)
    {
        var size = Setup<Half>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadHalf();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestFloat(float value, Endianness endianness)
    {
        var size = Setup<float>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadFloat();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestDouble(double value, Endianness endianness)
    {
        var size = Setup<double>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadDouble();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestVector2(Vector2 value, Endianness endianness)
    {
        var size = Setup<Vector2>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadVector2();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestVector3(Vector3 value, Endianness endianness)
    {
        var size = Setup<Vector3>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadVector3();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestVector4(Vector4 value, Endianness endianness)
    {
        var size = Setup<Vector4>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadVector4();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestQuaternion(Quaternion value, Endianness endianness)
    {
        var size = Setup<Quaternion>(endianness, out var writer, out var reader);
        writer.Write(value);
        writer.WrittenCount.Should().Be(size);
        var read = reader.ReadQuaternion();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestGuid(Guid value, Guid read, Endianness endianness)
    {
        var size = Setup<Guid>(endianness, out var writer, out var reader);
        writer.Write(value);
        reader.Read(ref read);
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestTimeSpan(TimeSpan value, TimeSpan read, Endianness endianness)
    {
        var size = Setup<TimeSpan>(endianness, out var writer, out var reader);
        writer.Write(value);
        reader.Read(ref read);
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestDateTime(DateTime value, DateTime read, Endianness endianness)
    {
        const int kindSize = 1;
        var size = Setup<DateTime>(endianness, out var writer, out var reader, kindSize);
        writer.Write(value);
        reader.Read(ref read);
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestDateTimeOffset(DateTimeOffset value, DateTimeOffset read, Endianness endianness)
    {
        var size = Setup<DateTimeOffset>(endianness, out var writer, out var reader);
        writer.Write(value);
        reader.Read(ref read);
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestDateOnly(DateOnly value, DateOnly read, Endianness endianness)
    {
        var size = Setup<DateOnly>(endianness, out var writer, out var reader);
        writer.Write(value);
        reader.Read(ref read);
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool TestFrame(Frame value, Frame read, Endianness endianness)
    {
        var size = Setup<Frame>(endianness, out var writer, out var reader);
        writer.Write(value);
        reader.Read(ref read);
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    [PropertyTest]
    public bool UnmanagedStruct(SimpleStructData value, Endianness endianness)
    {
        var size = Setup<SimpleStructData>(endianness, out var writer, out var reader);
        writer.WriteStruct(in value);
        writer.WrittenCount.Should().Be(size);

        var read = reader.ReadStruct<SimpleStructData>();
        reader.ReadCount.Should().Be(size);
        return value == read;
    }

    static int writeOffset;
    static int readOffset;

    static int Setup<T>(
        Endianness endianness,
        out BinarySpanWriter writer,
        out BinaryBufferReader reader,
        int extra = 0
    ) where T : struct
    {
        var size = Unsafe.SizeOf<T>() + extra;
        Span<byte> buffer = new byte[size];
        writeOffset = 0;
        readOffset = 0;
        writer = new(buffer, ref writeOffset, endianness);
        reader = new(buffer, ref readOffset, endianness);
        return size;
    }

    [Collection(SerialCollectionDefinition.Name)]
    public class ReadWriteBinaryIntegerTests
    {
        [PropertyTest] public bool TestInt(int value, Endianness endianness) => TestInteger(value, endianness);
        [PropertyTest] public bool TestUInt(uint value, Endianness endianness) => TestInteger(value, endianness);
        [PropertyTest] public bool TestLong(long value, Endianness endianness) => TestInteger(value, endianness);

        [PropertyTest]
        public bool TestULong(ulong value, Endianness endianness) =>
            TestInteger(value, endianness);

        [PropertyTest]
        public bool TestShort(short value, Endianness endianness) =>
            TestInteger(value, endianness);

        [PropertyTest]
        public bool TestUShort(ushort value, Endianness endianness) =>
            TestInteger(value, endianness);

        [PropertyTest] public bool TestByte(byte value, Endianness endianness) => TestInteger(value, endianness);

        [PropertyTest]
        public bool TestSByte(sbyte value, Endianness endianness) =>
            TestInteger(value, endianness);

        static bool TestInteger<T>(T value, Endianness endianness)
            where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
        {
            var size = Setup<T>(endianness, out var writer, out var reader);
            writer.WriteNumber(value);
            writer.WrittenCount.Should().Be(size);
            var read = reader.ReadNumber<T>();
            reader.ReadCount.Should().Be(size);
            return EqualityComparer<T>.Default.Equals(read, value);
        }
    }
}
