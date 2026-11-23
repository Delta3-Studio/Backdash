using System.Buffers;
using Backdash.Network;
using Backdash.Serialization;
using BenchmarkDotNet.Configs;

namespace Backdash.Benchmarks.Cases;

[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EndiannessSerializationScalarBenchmarks
{
    [Params(Endianness.LittleEndian, Endianness.BigEndian)]
    public Endianness ByteOrder { get; set; }

    IEndiannessSerializer oldSerializer = null!;
    EndiannessNumberSerializer newSerializer;

    const int NumberSize = sizeof(long);
    readonly byte[] oldNumberBuffer = new byte[NumberSize];
    readonly byte[] newNumberBuffer = new byte[NumberSize];

    const int GuidSize = 16;
    readonly byte[] oldGuidBuffer = new byte[GuidSize];
    readonly byte[] newGuidBuffer = new byte[GuidSize];

    ArrayBufferWriter<byte> oldBufferWriter = null!;
    ArrayBufferWriter<byte> newBufferWriter = null!;

    Guid guid;
    const long Number = 0x123456789ABCDEF;

    [GlobalSetup]
    public void Setup()
    {
        Random random = new(42);
        guid = random.Generate<Guid>();

        oldSerializer = EndiannessSerializerOld.Get(ByteOrder);
        newSerializer = EndiannessSerializer.Get(ByteOrder);

        oldBufferWriter = new(GuidSize);
        newBufferWriter = new(GuidSize);

        ValidateSerialization();
    }

    void ValidateSerialization()
    {
        oldSerializer.Write(oldNumberBuffer, Number, out _);
        newSerializer.Write(newNumberBuffer, Number, out _);
        Ensure.Equal(oldNumberBuffer, newNumberBuffer, "Number");

        oldSerializer.Write(oldGuidBuffer, in guid, out _);
        newSerializer.Write(newGuidBuffer, in guid, out _);
        Ensure.Equal(oldGuidBuffer, newGuidBuffer, "Guid");

        if (oldSerializer.Read<long>(oldNumberBuffer, false, out _) !=
            newSerializer.Read<long>(newNumberBuffer, false, out _))
            throw new InvalidOperationException("Number deserialization mismatch.");

        if (oldSerializer.ReadGuid(oldGuidBuffer, out _) != newSerializer.ReadGuid(newGuidBuffer, out _))
            throw new InvalidOperationException("Guid deserialization mismatch.");

        if (!oldSerializer.Write(oldBufferWriter, Number) || !newSerializer.Write(newBufferWriter, Number))
            throw new InvalidOperationException("Number buffer write failed.");

        Ensure.Equal(oldBufferWriter, newBufferWriter, "BufferWriter number");

        oldBufferWriter.Clear();
        newBufferWriter.Clear();

        if (!oldSerializer.Write(oldBufferWriter, in guid) || !newSerializer.Write(newBufferWriter, in guid))
            throw new InvalidOperationException("Guid buffer write failed.");

        Ensure.Equal(oldBufferWriter, newBufferWriter, "BufferWriter Guid");
        oldBufferWriter.Clear();
        newBufferWriter.Clear();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Factory")]
    public IEndiannessSerializer Old_Get() => EndiannessSerializerOld.Get(ByteOrder);

    [Benchmark]
    [BenchmarkCategory("Factory")]
    public EndiannessNumberSerializer New_Get() => EndiannessSerializer.Get(ByteOrder);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Number.Write")]
    public bool Old_NumberWrite() => oldSerializer.Write(oldNumberBuffer, Number, out _);

    [Benchmark]
    [BenchmarkCategory("Number.Write")]
    public bool New_NumberWrite() => newSerializer.Write(newNumberBuffer, Number, out _);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Number.Read")]
    public long Old_NumberRead() => oldSerializer.Read<long>(oldNumberBuffer, false, out _);

    [Benchmark]
    [BenchmarkCategory("Number.Read")]
    public long New_NumberRead() => newSerializer.Read<long>(newNumberBuffer, false, out _);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Number.ReadRef")]
    public long Old_NumberReadRef()
    {
        var result = 0L;
        oldSerializer.Read(ref result, oldNumberBuffer, false, out _);
        return result;
    }

    [Benchmark]
    [BenchmarkCategory("Number.ReadRef")]
    public long New_NumberReadRef()
    {
        var result = 0L;
        newSerializer.Read(ref result, newNumberBuffer, false, out _);
        return result;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Guid.Write")]
    public bool Old_GuidWrite() => oldSerializer.Write(oldGuidBuffer, in guid, out _);

    [Benchmark]
    [BenchmarkCategory("Guid.Write")]
    public bool New_GuidWrite() => newSerializer.Write(newGuidBuffer, in guid, out _);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Guid.Read")]
    public Guid Old_GuidRead() => oldSerializer.ReadGuid(oldGuidBuffer, out _);

    [Benchmark]
    [BenchmarkCategory("Guid.Read")]
    public Guid New_GuidRead() => newSerializer.ReadGuid(newGuidBuffer, out _);

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BufferWriter.NumberWrite")]
    public int Old_BufferWriterNumberWrite()
    {
        oldBufferWriter.Clear();
        oldSerializer.Write(oldBufferWriter, Number);
        return oldBufferWriter.WrittenCount;
    }

    [Benchmark]
    [BenchmarkCategory("BufferWriter.NumberWrite")]
    public int New_BufferWriterNumberWrite()
    {
        newBufferWriter.Clear();
        newSerializer.Write(newBufferWriter, Number);
        return newBufferWriter.WrittenCount;
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BufferWriter.GuidWrite")]
    public int Old_BufferWriterGuidWrite()
    {
        oldBufferWriter.Clear();
        oldSerializer.Write(oldBufferWriter, in guid);
        return oldBufferWriter.WrittenCount;
    }

    [Benchmark]
    [BenchmarkCategory("BufferWriter.GuidWrite")]
    public int New_BufferWriterGuidWrite()
    {
        newBufferWriter.Clear();
        newSerializer.Write(newBufferWriter, in guid);
        return newBufferWriter.WrittenCount;
    }
}

[MemoryDiagnoser]
[DisassemblyDiagnoser(maxDepth: 2)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EndiannessSerializationBufferBenchmarks
{
    [Params(Endianness.LittleEndian, Endianness.BigEndian)]
    public Endianness ByteOrder { get; set; }

    [Params(8, 64, 1024, 1 << 14, 1 << 20)]
    public int Count { get; set; }

    IEndiannessSerializer oldSerializer = null!;
    EndiannessNumberSerializer newSerializer;

    int[] values = [];
    int[] oldReadValues = [];
    int[] newReadValues = [];

    byte[] oldSpanWriteBuffer = [];
    byte[] newSpanWriteBuffer = [];

    ArrayBufferWriter<byte> oldBufferWriter = null!;
    ArrayBufferWriter<byte> newBufferWriter = null!;

    [GlobalSetup]
    public void Setup()
    {
        values = new int[Count];
        Random random = new(42);
        random.Generate(values);

        oldReadValues = new int[Count];
        newReadValues = new int[Count];

        oldSerializer = EndiannessSerializerOld.Get(ByteOrder);
        newSerializer = EndiannessSerializer.Get(ByteOrder);

        var bufferSize = Count * sizeof(int);

        oldSpanWriteBuffer = new byte[bufferSize];
        newSpanWriteBuffer = new byte[bufferSize];

        oldBufferWriter = new(bufferSize);
        newBufferWriter = new(bufferSize);

        ValidateSerialization();
    }

    void ValidateSerialization()
    {
        ReadOnlySpan<int> source = values;

        oldSerializer.Buffer.Write(oldSpanWriteBuffer, in source);
        newSerializer.Write(newSpanWriteBuffer, in source);
        Ensure.Equal(oldSpanWriteBuffer, newSpanWriteBuffer, "Span");

        ReadOnlySpan<byte> oldSource = oldSpanWriteBuffer;
        ReadOnlySpan<byte> newSource = newSpanWriteBuffer;

        Span<int> oldDestination = oldReadValues;
        Span<int> newDestination = newReadValues;
        oldSerializer.Buffer.Read(oldSource, in oldDestination);
        newSerializer.Read(newSource, in newDestination);

        if (!oldReadValues.AsSpan().SequenceEqual(values))
            throw new InvalidOperationException("Old span deserialization mismatch.");

        if (!newReadValues.AsSpan().SequenceEqual(values))
            throw new InvalidOperationException("New span deserialization mismatch.");

        oldBufferWriter.Clear();
        newBufferWriter.Clear();

        oldSerializer.Buffer.Write(oldBufferWriter, in source);
        newSerializer.Write(newBufferWriter, in source);
        Ensure.Equal(oldBufferWriter, newBufferWriter, "BufferWriter span");

        oldBufferWriter.Clear();
        newBufferWriter.Clear();
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Span.Write")]
    public int Old_SpanWrite()
    {
        ReadOnlySpan<int> source = values;
        return oldSerializer.Buffer.Write(oldSpanWriteBuffer, in source);
    }

    [Benchmark]
    [BenchmarkCategory("Span.Write")]
    public int New_SpanWrite()
    {
        ReadOnlySpan<int> source = values;
        return newSerializer.Write(newSpanWriteBuffer, in source);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Span.Read")]
    public int Old_SpanRead()
    {
        ReadOnlySpan<byte> source = oldSpanWriteBuffer;
        Span<int> destination = oldReadValues;

        var bytesRead = oldSerializer.Buffer.Read(source, in destination);
        return bytesRead ^ destination[0] ^ destination[^1];
    }

    [Benchmark]
    [BenchmarkCategory("Span.Read")]
    public int New_SpanRead()
    {
        ReadOnlySpan<byte> source = newSpanWriteBuffer;
        Span<int> destination = newReadValues;

        var bytesRead = newSerializer.Read(source, in destination);
        return bytesRead ^ destination[0] ^ destination[^1];
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BufferWriter.SpanWrite")]
    public int Old_BufferWriterSpanWrite()
    {
        oldBufferWriter.Clear();
        ReadOnlySpan<int> source = values;
        oldSerializer.Buffer.Write(oldBufferWriter, in source);
        return oldBufferWriter.WrittenCount;
    }

    [Benchmark]
    [BenchmarkCategory("BufferWriter.SpanWrite")]
    public int New_BufferWriterSpanWrite()
    {
        newBufferWriter.Clear();
        ReadOnlySpan<int> source = values;
        newSerializer.Write(newBufferWriter, in source);
        return newBufferWriter.WrittenCount;
    }
}
