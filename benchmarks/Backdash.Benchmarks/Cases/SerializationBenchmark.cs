// ReSharper disable UnassignedField.Global, NonReadonlyMemberInGetHashCode

#pragma warning disable S2328, S4035

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Backdash.Data;
using Backdash.Network;
using Backdash.Serialization;
using Backdash.Serialization.Internal;

namespace Backdash.Benchmarks.Cases;

[InProcess, MemoryDiagnoser, RankColumn]
public class SerializationBenchmark
{
    TestData data = null!;
    TestData result = null!;
    EndiannessSerializer.INumberSerializer numberSerializer = null!;
    ArrayBufferWriter<byte> buffer = null!;

    [Params(Endianness.LittleEndian, Endianness.BigEndian)]
    public Endianness SerializationEndianness;

    [Params(true, false)]
    public bool WithRef;

    [Params(true, false)]
    public bool WithIn;

    const int TestItemCount = 1_000_000;
    const int TestDataSize = 100;

    [GlobalSetup]
    public void Setup()
    {
        var bufferSize = ByteSize.NextPowerOfTwo(ByteSize.OfType<TestEntryData>() * (TestItemCount + 1));
        Console.WriteLine($"===> Current-Endianness: {Platform.Endianness}, Buffer-Size: {bufferSize})");

        Random random = new(42);
        data = new(TestItemCount, random, WithIn, WithRef);
        buffer = new((int)bufferSize.ByteCount);
    }

    [IterationSetup]
    public void BeforeEach()
    {
        buffer.Clear();
        numberSerializer = EndiannessSerializer.Get(SerializationEndianness);
        result = new(TestItemCount, WithIn, WithRef);
    }

    [IterationCleanup]
    public void AfterEach()
    {
        var size = ByteSize.FromBytes(buffer.WrittenCount);
        Console.WriteLine($"===> Data-Size: {size} ({size.ByteCount} bytes), Endianness: {SerializationEndianness}");
    }

    [Benchmark]
    public void Backdash()
    {
        BinaryBufferWriter writer = new(buffer, numberSerializer);
        writer.Write(data);
        int offset = 0;
        BinaryBufferReader reader = new(buffer.WrittenSpan, ref offset, numberSerializer);
        reader.Read(result);
        Debug.Assert(data == result);
    }

    public sealed class TestData(int itemsSize, bool useIn, bool useRef) : IBinarySerializable, IEquatable<TestData>
    {
        public bool Field1;
        public ulong Field2;
        public readonly TestEntryData[] Field3 = new TestEntryData[itemsSize];

        public TestData(int itemsSize, Random random, bool useIn, bool useRef) : this(itemsSize, useIn, useRef)
        {
            Field1 = random.NextBool();
            Field2 = random.Next<ulong>();

            for (int i = 0; i < Field3.Length; i++)
                Field3[i] = new(random, useIn, useRef);
        }

        public void Serialize(ref readonly BinaryBufferWriter writer)
        {
            if (useIn)
            {
                writer.Write(in Field1);
                writer.Write(in Field2);
                writer.Write(in Field3);
            }
            else
            {
                writer.Write(Field1);
                writer.Write(Field2);
                writer.Write(Field3);
            }
        }

        public void Deserialize(ref readonly BinaryBufferReader reader)
        {
            if (useRef)
            {
                reader.Read(ref Field1);
                reader.Read(ref Field2);
            }
            else
            {
                Field1 = reader.ReadBoolean();
                Field2 = reader.ReadUInt64();
            }

            reader.Read(in Field3);
        }

        public override int GetHashCode() => throw new InvalidOperationException();

        public bool Equals(TestData? other) => Equals(this, other);

        public override bool Equals(object? obj) =>
            ReferenceEquals(this, obj) || (obj is TestData other && Equals(other));

        public static bool Equals(TestData? left, TestData? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;

            return left.Field1 == right.Field1
                   && left.Field2 == right.Field2
                   && left.Field3.AsSpan().SequenceEqual(right.Field3);
        }

        public static bool operator ==(TestData? left, TestData? right) => Equals(left, right);
        public static bool operator !=(TestData? left, TestData? right) => !Equals(left, right);
    }

    public struct TestEntryData(bool useIn, bool useRef) : IBinarySerializable, IEquatable<TestEntryData>
    {
        public int Field1;
        public uint Field2;
        public ulong Field3;
        public long Field4;
        public short Field5;
        public ushort Field6;
        public byte Field7;
        public sbyte Field8;
        public Int128 Field9;
        public TestDataValues Field10;

        public TestEntryData(Random random, bool useIn, bool useRef) : this(useIn, useRef)
        {
            Field1 = random.Next();
            Field2 = random.Next<uint>();
            Field3 = random.Next<ulong>();
            Field4 = random.Next<long>();
            Field5 = random.Next<short>();
            Field6 = random.Next<ushort>();
            Field7 = random.Next<byte>();
            Field8 = random.Next<sbyte>();
            Field9 = random.Next<Int128>();
            Field10 = new(random);
        }

        public readonly void Serialize(ref readonly BinaryBufferWriter writer)
        {
            if (useIn)
            {
                writer.Write(in Field1);
                writer.Write(in Field2);
                writer.Write(in Field3);
                writer.Write(in Field4);
                writer.Write(in Field5);
                writer.Write(in Field6);
                writer.Write(in Field7);
                writer.Write(in Field8);
                writer.Write(in Field9);
                writer.Write(Field10);
            }
            else
            {
                writer.Write(Field1);
                writer.Write(Field2);
                writer.Write(Field3);
                writer.Write(Field4);
                writer.Write(Field5);
                writer.Write(Field6);
                writer.Write(Field7);
                writer.Write(Field8);
                writer.Write(Field9);
                writer.Write(Field10);
            }
        }

        public void Deserialize(ref readonly BinaryBufferReader reader)
        {
            if (useRef)
            {
                reader.Read(ref Field1);
                reader.Read(ref Field2);
                reader.Read(ref Field3);
                reader.Read(ref Field4);
                reader.Read(ref Field5);
                reader.Read(ref Field6);
                reader.Read(ref Field7);
                reader.Read(ref Field8);
                reader.Read(ref Field9);
            }
            else
            {
                Field1 = reader.ReadInt32();
                Field2 = reader.ReadUInt32();
                Field3 = reader.ReadUInt64();
                Field4 = reader.ReadInt64();
                Field5 = reader.ReadInt16();
                Field6 = reader.ReadUInt16();
                Field7 = reader.ReadByte();
                Field8 = reader.ReadSByte();
                Field9 = reader.ReadInt128();
            }

            reader.Read(Field10);
        }

        public override readonly int GetHashCode() => throw new InvalidOperationException();

        public readonly bool Equals(TestEntryData other) => Equals(in this, in other);
        public override readonly bool Equals(object? obj) => obj is TestEntryData other && Equals(in this, in other);

        public static bool Equals(in TestEntryData left, in TestEntryData right) =>
            left.Field1 == right.Field1 &&
            left.Field2 == right.Field2 &&
            left.Field3 == right.Field3 &&
            left.Field4 == right.Field4 &&
            left.Field5 == right.Field5 &&
            left.Field6 == right.Field6 &&
            left.Field7 == right.Field7 &&
            left.Field8 == right.Field8 &&
            left.Field9 == right.Field9 &&
            ((ReadOnlySpan<int>)left.Field10).SequenceEqual(right.Field10);

        public static bool operator ==(TestEntryData left, TestEntryData right) => Equals(in left, in right);
        public static bool operator !=(TestEntryData left, TestEntryData right) => !Equals(in left, in right);
    }

    [InlineArray(TestDataSize)]
    public struct TestDataValues
    {
        int element0;
        public TestDataValues(Random random) => random.Next((Span<int>)this);
    }
}
