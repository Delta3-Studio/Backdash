using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backdash.Core;
using Backdash.Network;

namespace Backdash.Serialization;

/// <summary>
/// Get endianness specific number serializer
/// </summary>
public static class EndiannessSerializer
{
    /// <summary>
    /// Get endianness serializer singleton
    /// </summary>
    public static IEndiannessSerializer Get(Endianness endianness) => endianness switch
    {
        Endianness.LittleEndian => LittleEndian.Instance,
        Endianness.BigEndian => BigEndian.Instance,
        _ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
    };

    static bool NeedsReverse(Endianness endianness) => Platform.Endianness != endianness;

    static IEndiannessSpanSerializer CreateSpanSerializer(Endianness endianness) =>
        NeedsReverse(endianness)
            ? new ReverseEndiannessSpanSerializer()
            : new NormalEndiannessSpanSerializer();

    internal sealed class BigEndian : IEndiannessSerializer
    {
        public static readonly BigEndian Instance = new();
        const Endianness BaseEndianness = Endianness.BigEndian;
        public Endianness Endianness => BaseEndianness;
        public IEndiannessSpanSerializer Span { get; } = CreateSpanSerializer(BaseEndianness);

        public T Read<T>(ReadOnlySpan<byte> buffer, bool isUnsigned, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            size = Unsafe.SizeOf<T>();
            return T.ReadBigEndian(buffer[..size], isUnsigned);
        }

        public void Read<T>(ref T value, ReadOnlySpan<byte> buffer, bool isUnsigned, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            size = Unsafe.SizeOf<T>();
            if (!T.TryReadBigEndian(buffer[..size], isUnsigned, out value))
                throw new OverflowException();
        }

        public Guid ReadGuid(ReadOnlySpan<byte> buffer, out int size)
        {
            size = Unsafe.SizeOf<Guid>();
            return new(buffer[..size], bigEndian: true);
        }

        public bool Write<T>(Span<byte> buffer, T value, out int size)
            where T : unmanaged, IBinaryInteger<T> =>
            value.TryWriteBigEndian(buffer, out size);

        public bool Write<T>(ArrayBufferWriter<byte> buffer, T value)
            where T : unmanaged, IBinaryInteger<T>
        {
            var size = Unsafe.SizeOf<T>();
            if (!value.TryWriteBigEndian(buffer.ReadSpan(size), out size)) return false;
            buffer.Advance(size);
            return true;
        }

        public bool Write(Span<byte> buffer, in Guid value, out int size) =>
            value.TryWriteBytes(buffer, bigEndian: true, out size);

        public bool Write(ArrayBufferWriter<byte> buffer, in Guid value)
        {
            var span = buffer.ReadSpan(Unsafe.SizeOf<Guid>());
            if (!value.TryWriteBytes(span, bigEndian: true, out var size)) return false;
            buffer.Advance(size);
            return true;
        }
    }

    internal sealed class LittleEndian : IEndiannessSerializer
    {
        public static readonly LittleEndian Instance = new();
        const Endianness BaseEndianness = Endianness.LittleEndian;
        public Endianness Endianness => BaseEndianness;
        public IEndiannessSpanSerializer Span { get; } = CreateSpanSerializer(BaseEndianness);

        public T Read<T>(ReadOnlySpan<byte> buffer, bool isUnsigned, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            size = Unsafe.SizeOf<T>();
            return T.ReadLittleEndian(buffer[..size], isUnsigned);
        }

        public void Read<T>(ref T value, ReadOnlySpan<byte> buffer, bool isUnsigned, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            size = Unsafe.SizeOf<T>();
            if (!T.TryReadLittleEndian(buffer[..size], isUnsigned, out value))
                throw new OverflowException();
        }

        public Guid ReadGuid(ReadOnlySpan<byte> buffer, out int size)
        {
            size = Unsafe.SizeOf<Guid>();
            return new(buffer[..size], bigEndian: false);
        }

        public bool Write<T>(Span<byte> buffer, T value, out int size)
            where T : unmanaged, IBinaryInteger<T> =>
            value.TryWriteLittleEndian(buffer, out size);

        public bool Write<T>(ArrayBufferWriter<byte> buffer, T value)
            where T : unmanaged, IBinaryInteger<T>
        {
            var size = Unsafe.SizeOf<T>();
            if (!value.TryWriteLittleEndian(buffer.ReadSpan(size), out size)) return false;
            buffer.Advance(size);
            return true;
        }

        public bool Write(Span<byte> buffer, in Guid value, out int size) =>
            value.TryWriteBytes(buffer, bigEndian: false, out size);

        public bool Write(ArrayBufferWriter<byte> buffer, in Guid value)
        {
            var span = buffer.ReadSpan(Unsafe.SizeOf<Guid>());
            if (!value.TryWriteBytes(span, bigEndian: false, out var size)) return false;
            buffer.Advance(size);
            return true;
        }
    }

    sealed class ReverseEndiannessSpanSerializer : IEndiannessSpanSerializer
    {
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<short> value) =>
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<short>(buffer, value.Length));

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ushort> value) =>
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<ushort>(buffer, value.Length));

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<int> value) =>
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<int>(buffer, value.Length));

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<uint> value) =>
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<uint>(buffer, value.Length));

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<long> value) =>
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<long>(buffer, value.Length));

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ulong> value) =>
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<ulong>(buffer, value.Length));

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<Int128> value) =>
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<Int128>(buffer, value.Length));

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<UInt128> value) =>
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<UInt128>(buffer, value.Length));

        public int Write(Span<byte> buffer, in ReadOnlySpan<short> value)
        {
            BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<short>(buffer, value.Length, out var size));
            return size;
        }

        public int Write(Span<byte> buffer, in ReadOnlySpan<ushort> value)
        {
            BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<ushort>(buffer, value.Length, out var size));
            return size;
        }

        public int Write(Span<byte> buffer, in ReadOnlySpan<int> value)
        {
            BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<int>(buffer, value.Length, out var size));
            return size;
        }

        public int Write(Span<byte> buffer, in ReadOnlySpan<uint> value)
        {
            BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<uint>(buffer, value.Length, out var size));
            return size;
        }

        public int Write(Span<byte> buffer, in ReadOnlySpan<long> value)
        {
            BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<long>(buffer, value.Length, out var size));
            return size;
        }

        public int Write(Span<byte> buffer, in ReadOnlySpan<ulong> value)
        {
            BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<ulong>(buffer, value.Length, out var size));
            return size;
        }

        public int Write(Span<byte> buffer, in ReadOnlySpan<Int128> value)
        {
            BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<Int128>(buffer, value.Length, out var size));
            return size;
        }

        public int Write(Span<byte> buffer, in ReadOnlySpan<UInt128> value)
        {
            BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<UInt128>(buffer, value.Length, out var size));
            return size;
        }

        public int Read(ReadOnlySpan<byte> buffer, in Span<short> values)
        {
            var data = Mem.CastSpan<short>(buffer, values.Length, out var size);
            BinaryPrimitives.ReverseEndianness(data, values);
            return size;
        }

        public int Read(ReadOnlySpan<byte> buffer, in Span<ushort> values)
        {
            var data = Mem.CastSpan<ushort>(buffer, values.Length, out var size);
            BinaryPrimitives.ReverseEndianness(data, values);
            return size;
        }

        public int Read(ReadOnlySpan<byte> buffer, in Span<int> values)
        {
            var data = Mem.CastSpan<int>(buffer, values.Length, out var size);
            BinaryPrimitives.ReverseEndianness(data, values);
            return size;
        }

        public int Read(ReadOnlySpan<byte> buffer, in Span<uint> values)
        {
            var data = Mem.CastSpan<uint>(buffer, values.Length, out var size);
            BinaryPrimitives.ReverseEndianness(data, values);
            return size;
        }

        public int Read(ReadOnlySpan<byte> buffer, in Span<long> values)
        {
            var data = Mem.CastSpan<long>(buffer, values.Length, out var size);
            BinaryPrimitives.ReverseEndianness(data, values);
            return size;
        }

        public int Read(ReadOnlySpan<byte> buffer, in Span<ulong> values)
        {
            var data = Mem.CastSpan<ulong>(buffer, values.Length, out var size);
            BinaryPrimitives.ReverseEndianness(data, values);
            return size;
        }

        public int Read(ReadOnlySpan<byte> buffer, in Span<Int128> values)
        {
            var data = Mem.CastSpan<Int128>(buffer, values.Length, out var size);
            BinaryPrimitives.ReverseEndianness(data, values);
            return size;
        }

        public int Read(ReadOnlySpan<byte> buffer, in Span<UInt128> values)
        {
            var data = Mem.CastSpan<UInt128>(buffer, values.Length, out var size);
            BinaryPrimitives.ReverseEndianness(data, values);
            return size;
        }
    }

    sealed class NormalEndiannessSpanSerializer : IEndiannessSpanSerializer
    {
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<short> value) => Mem.WriteSpan(buffer, value);

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ushort> value) =>
            Mem.WriteSpan(buffer, value);

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<int> value) => Mem.WriteSpan(buffer, value);
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<uint> value) => Mem.WriteSpan(buffer, value);
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<long> value) => Mem.WriteSpan(buffer, value);
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ulong> value) => Mem.WriteSpan(buffer, value);

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<Int128> value) =>
            Mem.WriteSpan(buffer, value);

        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<UInt128> value) =>
            Mem.WriteSpan(buffer, value);

        public int Write(Span<byte> buffer, in ReadOnlySpan<short> value) => Mem.WriteSpan(buffer, value);
        public int Write(Span<byte> buffer, in ReadOnlySpan<ushort> value) => Mem.WriteSpan(buffer, value);
        public int Write(Span<byte> buffer, in ReadOnlySpan<int> value) => Mem.WriteSpan(buffer, value);
        public int Write(Span<byte> buffer, in ReadOnlySpan<uint> value) => Mem.WriteSpan(buffer, value);
        public int Write(Span<byte> buffer, in ReadOnlySpan<long> value) => Mem.WriteSpan(buffer, value);
        public int Write(Span<byte> buffer, in ReadOnlySpan<ulong> value) => Mem.WriteSpan(buffer, value);
        public int Write(Span<byte> buffer, in ReadOnlySpan<Int128> value) => Mem.WriteSpan(buffer, value);
        public int Write(Span<byte> buffer, in ReadOnlySpan<UInt128> value) => Mem.WriteSpan(buffer, value);
        public int Read(ReadOnlySpan<byte> buffer, in Span<short> values) => Mem.ReadSpan(buffer, values);
        public int Read(ReadOnlySpan<byte> buffer, in Span<ushort> values) => Mem.ReadSpan(buffer, values);
        public int Read(ReadOnlySpan<byte> buffer, in Span<int> values) => Mem.ReadSpan(buffer, values);
        public int Read(ReadOnlySpan<byte> buffer, in Span<uint> values) => Mem.ReadSpan(buffer, values);
        public int Read(ReadOnlySpan<byte> buffer, in Span<long> values) => Mem.ReadSpan(buffer, values);
        public int Read(ReadOnlySpan<byte> buffer, in Span<ulong> values) => Mem.ReadSpan(buffer, values);
        public int Read(ReadOnlySpan<byte> buffer, in Span<Int128> values) => Mem.ReadSpan(buffer, values);
        public int Read(ReadOnlySpan<byte> buffer, in Span<UInt128> values) => Mem.ReadSpan(buffer, values);
    }
}

/// <summary>
///  Endianness specific number serializer
/// </summary>
public interface IEndiannessSerializer
{
    /// <summary>Serializer endianness</summary>
    Endianness Endianness { get; }

    /// <summary>Span/Buffer value serializer</summary>
    IEndiannessSpanSerializer Span { get; }

    /// <summary>Write a number into a buffer</summary>
    bool Write<T>(Span<byte> buffer, T value, out int size) where T : unmanaged, IBinaryInteger<T>;

    /// <summary>Write a number into an array buffer</summary>
    bool Write<T>(ArrayBufferWriter<byte> buffer, T value)
        where T : unmanaged, IBinaryInteger<T>;

    /// <summary>Write a <see cref="Guid"/> into an array buffer</summary>
    bool Write(Span<byte> buffer, in Guid value, out int size);

    /// <summary>Write a <see cref="Guid"/> value into an array buffer</summary>
    bool Write(ArrayBufferWriter<byte> buffer, in Guid value);

    /// <summary>Reads number from the buffer</summary>
    T Read<T>(ReadOnlySpan<byte> buffer, bool isUnsigned, out int size) where T : unmanaged, IBinaryInteger<T>;

    /// <summary>Reads number from the buffer</summary>
    void Read<T>(ref T value, ReadOnlySpan<byte> buffer, bool isUnsigned, out int size)
        where T : unmanaged, IBinaryInteger<T>;

    /// <summary>Reads <see cref="Guid"/> value from the buffer</summary>
    Guid ReadGuid(ReadOnlySpan<byte> buffer, out int size);
}

/// <summary>
///  Endianness specific number serializer
/// </summary>
public interface IEndiannessSpanSerializer
{
    /// <summary>Writes a span of <see cref="short" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<short> value);

    /// <summary>Writes a span of <see cref="ushort" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ushort> value);

    /// <summary>Writes a span of <see cref="int" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<int> value);

    /// <summary>Writes a span of <see cref="uint" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<uint> value);

    /// <summary>Writes a span of <see cref="long" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<long> value);

    /// <summary>Writes a span of <see cref="ulong" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ulong> value);

    /// <summary>Writes a span of <see cref="Int128" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<Int128> value);

    /// <summary>Writes a span of <see cref="UInt128" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<UInt128> value);

    /// <summary>Writes a span of <see cref="short" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<short> value);

    /// <summary>Writes a span of <see cref="ushort" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<ushort> value);

    /// <summary>Writes a span of <see cref="int" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<int> value);

    /// <summary>Writes a span of <see cref="uint" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<uint> value);

    /// <summary>Writes a span of <see cref="long" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<long> value);

    /// <summary>Writes a span of <see cref="ulong" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<ulong> value);

    /// <summary>Writes a span of <see cref="Int128" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<Int128> value);

    /// <summary>Writes a span of <see cref="UInt128" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<UInt128> value);

    /// <summary>Reads a span of <see cref="short" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<short> values);

    /// <summary>Reads a span of <see cref="ushort" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<ushort> values);

    /// <summary>Reads a span of <see cref="int" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<int> values);

    /// <summary>Reads a span of <see cref="uint" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<uint> values);

    /// <summary>Reads a span of <see cref="long" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<long> values);

    /// <summary>Reads a span of <see cref="ulong" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<ulong> values);

    /// <summary>Reads a span of <see cref="Int128" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<Int128> values);

    /// <summary>Reads a span of <see cref="UInt128" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<UInt128> values);
}

/// <summary>
///  Endianness serializer extensions helpers
/// </summary>
public static class EndiannessSerializerEx
{
    extension(IEndiannessSerializer serializer)
    {
        /// <inheritdoc cref="IEndiannessSerializer.Read{T}(System.ReadOnlySpan{byte},bool,out int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(in ReadOnlySpan<byte> buffer, out int bytesRead)
            where T : unmanaged, ISignedNumber<T>, IBinaryInteger<T> =>
            serializer.Read<T>(buffer, false, out bytesRead);

        /// <inheritdoc cref="IEndiannessSerializer.Read{T}(System.ReadOnlySpan{byte},bool,out int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(ReadOnlySpan<byte> buffer, out int bytesToRead)
            where T : unmanaged, IUnsignedNumber<T>, IBinaryInteger<T> =>
            serializer.Read<T>(buffer, true, out bytesToRead);

        /// <summary>Reads single <see cref="Half" /> from the buffer.</summary>
        public Half ReadHalf(ReadOnlySpan<byte> buffer, out int size)
        {
            var num = serializer.Read<short>(buffer, false, out size);
            return BitConverter.Int16BitsToHalf(num);
        }

        /// <summary>Reads single <see cref="float" /> from the buffer.</summary>
        public float ReadFloat(ReadOnlySpan<byte> buffer, out int size)
        {
            var num = serializer.Read<int>(buffer, false, out size);
            return BitConverter.Int32BitsToSingle(num);
        }

        /// <summary>Reads single <see cref="float" /> from the buffer.</summary>
        public double ReadDouble(ReadOnlySpan<byte> buffer, out int size)
        {
            var num = serializer.Read<long>(buffer, false, out size);
            return BitConverter.Int64BitsToDouble(num);
        }

        /// <summary>Writes single <see cref="Half" /> <paramref name="value" /> into buffer.</summary>
        public bool Write(Span<byte> buffer, in Half value, out int size) =>
            serializer.Write(buffer, BitConverter.HalfToInt16Bits(value), out size);

        /// <summary>Writes single <see cref="float" /> <paramref name="value" /> into buffer.</summary>
        public bool Write(Span<byte> buffer, in float value, out int size) =>
            serializer.Write(buffer, BitConverter.SingleToInt32Bits(value), out size);

        /// <summary>Writes single <see cref="float" /> <paramref name="value" /> into buffer.</summary>
        public bool Write(Span<byte> buffer, in double value, out int size) =>
            serializer.Write(buffer, BitConverter.DoubleToInt64Bits(value), out size);

        /// <summary>Writes single <see cref="Half" /> <paramref name="value" /> into buffer.</summary>
        public bool Write(ArrayBufferWriter<byte> buffer, in Half value) =>
            serializer.Write(buffer, BitConverter.HalfToInt16Bits(value));

        /// <summary>Writes single <see cref="float" /> <paramref name="value" /> into buffer.</summary>
        public bool Write(ArrayBufferWriter<byte> buffer, in float value) =>
            serializer.Write(buffer, BitConverter.SingleToInt32Bits(value));

        /// <summary>Writes single <see cref="float" /> <paramref name="value" /> into buffer.</summary>
        public bool Write(ArrayBufferWriter<byte> buffer, in double value) =>
            serializer.Write(buffer, BitConverter.DoubleToInt64Bits(value));
    }

    extension(IEndiannessSpanSerializer serializer)
    {
        /// <summary>Reads a span of <see cref="char" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
        public int Read(ReadOnlySpan<byte> buffer, in Span<char> values) =>
            serializer.Read(buffer, MemoryMarshal.Cast<char, ushort>(values));

        /// <summary>Reads span of <see cref="Half" /> from the buffer.</summary>
        public int Read(ReadOnlySpan<byte> buffer, in Span<Half> values) =>
            serializer.Read(buffer, MemoryMarshal.Cast<Half, short>(values));

        /// <summary>Reads span of <see cref="float" /> from the buffer.</summary>
        public int Read(ReadOnlySpan<byte> buffer, in Span<float> values) =>
            serializer.Read(buffer, MemoryMarshal.Cast<float, int>(values));

        /// <summary>Reads span of <see cref="double" /> from the buffer.</summary>
        public int Read(ReadOnlySpan<byte> buffer, in Span<double> values) =>
            serializer.Read(buffer, MemoryMarshal.Cast<double, long>(values));

        /// <summary>Writes a span of <see cref="char" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<char> value) =>
            serializer.Write(buffer, MemoryMarshal.Cast<char, ushort>(value));

        /// <summary>Writes a span of <see cref="char" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
        /// <returns>The number of bytes written.</returns>
        public int Write(Span<byte> buffer, in ReadOnlySpan<char> value) =>
            serializer.Write(buffer, MemoryMarshal.Cast<char, ushort>(value));

        /// <summary>Writes a span of <see cref="float" /> <paramref name="values" /> into buffer.</summary>
        public int Write(Span<byte> buffer, in ReadOnlySpan<float> values) =>
            serializer.Write(buffer, MemoryMarshal.Cast<float, int>(values));

        /// <summary>Writes a span of <see cref="double" /> <paramref name="values" /> into buffer.</summary>
        public int Write(Span<byte> buffer, in ReadOnlySpan<double> values) =>
            serializer.Write(buffer, MemoryMarshal.Cast<double, long>(values));

        /// <summary>Writes a span of <see cref="Half" /> <paramref name="values" /> into buffer.</summary>
        public int Write(Span<byte> buffer, in ReadOnlySpan<Half> values) =>
            serializer.Write(buffer, MemoryMarshal.Cast<Half, short>(values));

        /// <summary>Writes a span of <see cref="float" /> <paramref name="values" /> into buffer.</summary>
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<float> values) =>
            serializer.Write(buffer, MemoryMarshal.Cast<float, int>(values));

        /// <summary>Writes a span of <see cref="double" /> <paramref name="values" /> into buffer.</summary>
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<double> values) =>
            serializer.Write(buffer, MemoryMarshal.Cast<double, long>(values));

        /// <summary>Writes a span of <see cref="Half" /> <paramref name="values" /> into buffer.</summary>
        public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<Half> values) =>
            serializer.Write(buffer, MemoryMarshal.Cast<Half, short>(values));
    }
}
