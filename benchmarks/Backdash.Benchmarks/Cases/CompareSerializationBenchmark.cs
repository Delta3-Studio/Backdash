// ReSharper disable UnassignedField.Global, NonReadonlyMemberInGetHashCode

#pragma warning disable S2328, S4035

using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backdash.Data;
using Backdash.Network;
using Backdash.Serialization;
using Backdash.Serialization.Internal;
using MemoryPack;

namespace Backdash.Benchmarks.Cases;

[RPlotExporter]
[InProcess, MemoryDiagnoser, RankColumn]
public class CompareSerializationBenchmark
{
    TestData data = null!;
    TestData result = null!;
    (BinaryReader Reader, BinaryWriter Writer) native;

    static int InitialBufferCapacity => (int)ByteSize.FromMebiBytes(10).ByteCount;
    readonly ArrayBufferWriter<byte> buffer = new(InitialBufferCapacity);
    readonly MemoryStream stream = new(InitialBufferCapacity);

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(42);
        data = TestData.Generate(random);
        native = new(new(stream), new(stream));
        Console.WriteLine($"===> Current-Endianness: {Platform.Endianness})");
    }

    [IterationSetup]
    public void BeforeEach()
    {
        result = new();
        buffer.Clear();
        stream.Seek(0, SeekOrigin.Begin);
    }

    [IterationCleanup]
    public void AfterEach()
    {
        var size = ByteSize.FromBytes(buffer.WrittenCount);
        Console.WriteLine($"Data-Size: {size} ({size.ByteCount} bytes)");
    }

    [Benchmark(Baseline = true)]
    public void NativeSerializer()
    {
        data.Serialize(native.Writer);
        stream.Seek(0, SeekOrigin.Begin);
        result.Deserialize(native.Reader);
        Debug.Assert(data == result);
    }

    [Benchmark]
    public void Backdash()
    {
        var endianness = EndiannessSerializer.LittleEndian.Instance;
        var writer = new BinaryBufferWriter(buffer, endianness);
        writer.Write(data);
        int offset = 0;
        var reader = new BinaryBufferReader(buffer.WrittenSpan, ref offset, endianness);
        reader.Read(result);
        Debug.Assert(data == result);
    }

    [Benchmark]
    public void Backdash_BigEndian()
    {
        var endianness = EndiannessSerializer.BigEndian.Instance;
        var writer = new BinaryBufferWriter(buffer, endianness);
        writer.Write(data);
        int offset = 0;
        var reader = new BinaryBufferReader(buffer.WrittenSpan, ref offset, endianness);
        reader.Read(result);
        Debug.Assert(data == result);
    }

    [Benchmark]
    public void MemoryPack()
    {
        MemoryPackSerializer.Serialize(buffer, data);
        MemoryPackSerializer.Deserialize(buffer.WrittenSpan, ref result!);
        Debug.Assert(data == result);
    }
}

[MemoryPackable]
public sealed partial class TestData : IBinarySerializable, INativeSerializable, IEquatable<TestData>
{
    public bool Field1;
    public ulong Field2;
    public byte[] Field3;
    public TestEntryData[] Field4;

    public TestData()
    {
        Field3 = new byte[100_000];
        Field4 = new TestEntryData[20_000];
        for (var i = 0; i < Field4.Length; i++)
            Field4[i].Field9 = new int[10_000];
    }

    public void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in Field1);
        writer.Write(in Field2);
        writer.Write(in Field3);
        writer.Write(in Field4);
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref Field1);
        reader.Read(ref Field2);
        reader.Read(Field3);
        reader.Read(Field4);
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(Field1);
        writer.Write(Field2);
        writer.Write(Field3);
        writer.WriteObject<TestEntryData>(Field4);
    }

    public void Deserialize(BinaryReader reader)
    {
        Field1 = reader.ReadBoolean();
        Field2 = reader.ReadUInt64();
        _ = reader.Read(Field3);
        reader.ReadObject<TestEntryData>(Field4.AsSpan());
    }

    public override int GetHashCode() => throw new InvalidOperationException();

    public bool Equals(TestData? other) => Equals(this, other);
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is TestData other && Equals(other));

    public static bool Equals(TestData? left, TestData? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;

        return left.Field1 == right.Field1
               && left.Field2 == right.Field2
               && left.Field3.AsSpan().SequenceEqual(right.Field3)
               && left.Field4.AsSpan().SequenceEqual(right.Field4);
    }

    public static bool operator ==(TestData? left, TestData? right) => Equals(left, right);
    public static bool operator !=(TestData? left, TestData? right) => !Equals(left, right);

    public static TestData Generate(Random random)
    {
        TestData testData = new()
        {
            Field1 = random.NextBool(),
            Field2 = random.Generate<ulong>(),
        };

        random.Generate(testData.Field3);

        for (int i = 0; i < testData.Field4.Length; i++)
        {
            ref var entry = ref testData.Field4[i];
            entry.Field1 = random.Next();
            entry.Field2 = random.Generate<uint>();
            entry.Field3 = random.Generate<ulong>();
            entry.Field4 = random.Generate<long>();
            entry.Field5 = random.Generate<short>();
            entry.Field6 = random.Generate<ushort>();
            entry.Field7 = random.Generate<byte>();
            entry.Field8 = random.Generate<sbyte>();
            random.Generate(entry.Field9.AsSpan());
        }

        return testData;
    }
}

[MemoryPackable]
public partial struct TestEntryData() : IBinarySerializable, INativeSerializable, IEquatable<TestEntryData>
{
    public int Field1;
    public uint Field2;
    public ulong Field3;
    public long Field4;
    public short Field5;
    public ushort Field6;
    public byte Field7;
    public sbyte Field8;
    public int[] Field9 = [];

    public readonly void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in Field1);
        writer.Write(in Field2);
        writer.Write(in Field3);
        writer.Write(in Field4);
        writer.Write(in Field5);
        writer.Write(in Field6);
        writer.Write(in Field7);
        writer.Write(in Field8);
        writer.Write(Field9);
    }

    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        Field1 = reader.ReadInt32();
        Field2 = reader.ReadUInt32();
        Field3 = reader.ReadUInt64();
        Field4 = reader.ReadInt64();
        Field5 = reader.ReadInt16();
        Field6 = reader.ReadUInt16();
        Field7 = reader.ReadByte();
        Field8 = reader.ReadSByte();
        reader.Read(Field9);
    }

    public readonly void Serialize(BinaryWriter writer)
    {
        writer.Write(Field1);
        writer.Write(Field2);
        writer.Write(Field3);
        writer.Write(Field4);
        writer.Write(Field5);
        writer.Write(Field6);
        writer.Write(Field7);
        writer.Write(Field8);
        writer.WriteSpan<int>(Field9);
    }

    public void Deserialize(BinaryReader reader)
    {
        Field1 = reader.ReadInt32();
        Field2 = reader.ReadUInt32();
        Field3 = reader.ReadUInt64();
        Field4 = reader.ReadInt64();
        Field5 = reader.ReadInt16();
        Field6 = reader.ReadUInt16();
        Field7 = reader.ReadByte();
        Field8 = reader.ReadSByte();
        reader.ReadSpan(Field9.AsSpan());
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
        left.Field9.AsSpan().SequenceEqual(right.Field9);

    public static bool operator ==(TestEntryData left, TestEntryData right) => Equals(in left, in right);
    public static bool operator !=(TestEntryData left, TestEntryData right) => !Equals(in left, in right);
}

interface INativeSerializable
{
    void Serialize(BinaryWriter writer);
    void Deserialize(BinaryReader reader);
}

static class BinaryWriterEx
{
    public static void WriteObject<T>(this BinaryWriter @this, T value) where T : INativeSerializable =>
        value.Serialize(@this);

    public static void WriteObject<T>(this BinaryWriter @this, ReadOnlySpan<T> values) where T : INativeSerializable
    {
        ref var current = ref MemoryMarshal.GetReference(values);
        ref var limit = ref Unsafe.Add(ref current, values.Length);

        while (Unsafe.IsAddressLessThan(ref current, ref limit))
        {
            current.Serialize(@this);
            current = ref Unsafe.Add(ref current, 1)!;
        }
    }

    public static void ReadObject<T>(this BinaryReader @this, T value) where T : INativeSerializable =>
        value.Deserialize(@this);

    public static void ReadObject<T>(this BinaryReader @this, ReadOnlySpan<T> values) where T : INativeSerializable
    {
        ref var current = ref MemoryMarshal.GetReference(values);
        ref var limit = ref Unsafe.Add(ref current, values.Length);

        while (Unsafe.IsAddressLessThan(ref current, ref limit))
        {
            current.Deserialize(@this);
            current = ref Unsafe.Add(ref current, 1)!;
        }
    }

    public static void WriteSpan<T>(this BinaryWriter @this, ReadOnlySpan<T> values) where T : unmanaged =>
        @this.Write(MemoryMarshal.AsBytes(values));

    public static void ReadSpan<T>(this BinaryReader @this, Span<T> values) where T : unmanaged =>
        _ = @this.Read(MemoryMarshal.AsBytes(values));
}
