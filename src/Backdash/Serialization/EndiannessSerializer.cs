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

    internal sealed class BigEndian : IEndiannessSerializer
    {
        public static readonly BigEndian Instance = new();
        const Endianness BaseEndianness = Endianness.BigEndian;

        public Endianness Endianness { get; } = BaseEndianness;

        public bool NeedsReverse { get; } = Platform.Endianness != BaseEndianness;

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
        public Endianness Endianness { get; } = BaseEndianness;

        public bool NeedsReverse { get; } = Platform.Endianness != BaseEndianness;

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
}

/// <summary>
///  Endianness specific number serializer
/// </summary>
public interface IEndiannessSerializer
{
    /// <summary>Serializer endianness</summary>
    Endianness Endianness { get; }

    /// <summary>True if it will be necessary to reverse the endianness</summary>
    bool NeedsReverse { get; }

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

    /// <inheritdoc cref="Read{T}(System.ReadOnlySpan{byte},bool,out int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T Read<T>(in ReadOnlySpan<byte> buffer, out int bytesRead)
        where T : unmanaged, ISignedNumber<T>, IBinaryInteger<T> =>
        Read<T>(buffer, false, out bytesRead);

    /// <inheritdoc cref="Read{T}(System.ReadOnlySpan{byte},bool,out int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    T Read<T>(ReadOnlySpan<byte> buffer, out int bytesToRead)
        where T : unmanaged, IUnsignedNumber<T>, IBinaryInteger<T> =>
        Read<T>(buffer, true, out bytesToRead);

    /// <summary>Reads <see cref="Guid"/> value from the buffer</summary>
    Guid ReadGuid(ReadOnlySpan<byte> buffer, out int size);
}

/// <summary>
///  Endianness specific number serializer
/// </summary>
public interface IEndiannessSpanSerializer
{
    /// <summary>Writes a span of <see cref="short" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<short> value);

    /// <summary>Writes a span of <see cref="ushort" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<ushort> value);

    /// <summary>Writes a span of <see cref="char" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    /// <returns>The number of bytes written.</returns>
    int Write(Span<byte> buffer, in ReadOnlySpan<char> value) =>
        Write(buffer, MemoryMarshal.Cast<char, ushort>(value));

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

    /// <summary>Writes a span of <see cref="short" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<short> value);

    /// <summary>Writes a span of <see cref="ushort" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ushort> value);

    /// <summary>Writes a span of <see cref="char" /> <paramref name="value" /> into <paramref name="buffer" />.</summary>
    void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<char> value) =>
        Write(buffer, MemoryMarshal.Cast<char, ushort>(value));

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

    /// <summary>Reads a span of <see cref="short" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<short> values);

    /// <summary>Reads a span of <see cref="char" /> <paramref name="buffer" /> buffer into <paramref name="values" />.</summary>
    int Read(ReadOnlySpan<byte> buffer, in Span<char> values) =>
        Read(buffer, MemoryMarshal.Cast<char, ushort>(values));

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

sealed class ReverseEndiannessSpanSerializer : IEndiannessSpanSerializer
{
    public int Write(Span<byte> buffer, in ReadOnlySpan<short> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<short>(buffer, value.Length, out var byteSize));
        return byteSize;
    }

    public int Write(Span<byte> buffer, in ReadOnlySpan<ushort> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<ushort>(buffer, value.Length, out var byteSize));
        return byteSize;
    }

    public int Write(Span<byte> buffer, in ReadOnlySpan<int> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<int>(buffer, value.Length, out var byteSize));
        return byteSize;
    }

    public int Write(Span<byte> buffer, in ReadOnlySpan<uint> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<uint>(buffer, value.Length, out var byteSize));
        return byteSize;
    }

    public int Write(Span<byte> buffer, in ReadOnlySpan<long> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<long>(buffer, value.Length, out var byteSize));
        return byteSize;
    }

    public int Write(Span<byte> buffer, in ReadOnlySpan<ulong> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<ulong>(buffer, value.Length, out var byteSize));
        return byteSize;
    }

    public int Write(Span<byte> buffer, in ReadOnlySpan<Int128> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<Int128>(buffer, value.Length, out var byteSize));
        return byteSize;
    }

    public int Write(Span<byte> buffer, in ReadOnlySpan<UInt128> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<UInt128>(buffer, value.Length, out var byteSize));
        return byteSize;
    }

    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<short> value)
    {
        BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<short>(buffer, value.Length));
    }

    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ushort> value)
    {
        throw new NotImplementedException();
    }

    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<int> value)
    {
        throw new NotImplementedException();
    }

    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<uint> value)
    {
        throw new NotImplementedException();
    }

    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<long> value)
    {
        throw new NotImplementedException();
    }

    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ulong> value)
    {
        throw new NotImplementedException();
    }

    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<Int128> value)
    {
        throw new NotImplementedException();
    }

    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<UInt128> value)
    {
        throw new NotImplementedException();
    }

    public int Read(ReadOnlySpan<byte> buffer, in Span<short> values)
    {
        throw new NotImplementedException();
    }

    public int Read(ReadOnlySpan<byte> buffer, in Span<ushort> values)
    {
        throw new NotImplementedException();
    }

    public int Read(ReadOnlySpan<byte> buffer, in Span<int> values)
    {
        throw new NotImplementedException();
    }

    public int Read(ReadOnlySpan<byte> buffer, in Span<uint> values)
    {
        throw new NotImplementedException();
    }

    public int Read(ReadOnlySpan<byte> buffer, in Span<long> values)
    {
        throw new NotImplementedException();
    }

    public int Read(ReadOnlySpan<byte> buffer, in Span<ulong> values)
    {
        throw new NotImplementedException();
    }

    public int Read(ReadOnlySpan<byte> buffer, in Span<Int128> values)
    {
        throw new NotImplementedException();
    }

    public int Read(ReadOnlySpan<byte> buffer, in Span<UInt128> values)
    {
        throw new NotImplementedException();
    }
}
