#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable S4136 // Adjacent methods with same name
// ReSharper disable MemberCanBePrivate.Global

using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backdash.Core;
using Backdash.Network;

namespace Backdash.Serialization;

/// <summary>
/// Endianness-specific number serializer.
/// </summary>
public static class EndiannessSerializer
{
    /// <summary>Gets an endianness-specific serializer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EndiannessNumberSerializer Get(Endianness endianness) =>
        endianness switch
        {
            Endianness.LittleEndian => LittleEndian,
            Endianness.BigEndian => BigEndian,
            _ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
        };

    /// <summary>Singleton <see cref="EndiannessNumberSerializer"/> instance for <see cref="Endianness.LittleEndian"/></summary>
    public static readonly EndiannessNumberSerializer LittleEndian = new(Endianness.LittleEndian);

    /// <summary>Singleton <see cref="EndiannessNumberSerializer"/> instance for <see cref="Endianness.BigEndian"/></summary>
    public static readonly EndiannessNumberSerializer BigEndian = new(Endianness.BigEndian);

    /// <summary>Singleton <see cref="EndiannessNumberSerializer"/> instance for current <see cref="Platform.Endianness"/></summary>
    public static readonly EndiannessNumberSerializer Current = new(Platform.Endianness);

    /// <summary>Indicate whether the endianness needs to be reversed.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NeedsReverse(Endianness endianness) => Platform.Endianness != endianness;
}

/// <summary>
/// Endianness-specific number serializer.
/// </summary>
public readonly struct EndiannessNumberSerializer
{
    readonly bool bigEndian;
    readonly bool reverse;

    internal EndiannessNumberSerializer(Endianness endianness)
    {
        bigEndian = endianness is Endianness.BigEndian;
        reverse = EndiannessSerializer.NeedsReverse(endianness);
    }

    /// <summary>Gets the serializer endianness.</summary>
    public Endianness Endianness => bigEndian ? Endianness.BigEndian : Endianness.LittleEndian;

    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public bool NeedsReverse => reverse;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>(ReadOnlySpan<byte> buffer, bool isUnsigned, out int size)
        where T : unmanaged, IBinaryInteger<T>
    {
        size = Unsafe.SizeOf<T>();

        return bigEndian
            ? T.ReadBigEndian(buffer[..size], isUnsigned)
            : T.ReadLittleEndian(buffer[..size], isUnsigned);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Read<T>(
        ref T value,
        ReadOnlySpan<byte> buffer,
        bool isUnsigned,
        out int size)
        where T : unmanaged, IBinaryInteger<T>
    {
        size = Unsafe.SizeOf<T>();

        var success = bigEndian
            ? T.TryReadBigEndian(buffer[..size], isUnsigned, out value)
            : T.TryReadLittleEndian(buffer[..size], isUnsigned, out value);

        if (!success)
            throw new OverflowException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>(in ReadOnlySpan<byte> buffer, out int bytesRead)
        where T : unmanaged, ISignedNumber<T>, IBinaryInteger<T> =>
        Read<T>(buffer, false, out bytesRead);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>(ReadOnlySpan<byte> buffer, out int bytesToRead)
        where T : unmanaged, IUnsignedNumber<T>, IBinaryInteger<T> =>
        Read<T>(buffer, true, out bytesToRead);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write<T>(Span<byte> buffer, T value, out int size)
        where T : unmanaged, IBinaryInteger<T> =>
        bigEndian
            ? value.TryWriteBigEndian(buffer, out size)
            : value.TryWriteLittleEndian(buffer, out size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write<T>(ArrayBufferWriter<byte> buffer, T value)
        where T : unmanaged, IBinaryInteger<T>
    {
        var span = buffer.ReadSpan(Unsafe.SizeOf<T>());

        var success = bigEndian
            ? value.TryWriteBigEndian(span, out var size)
            : value.TryWriteLittleEndian(span, out size);

        if (!success)
            return false;

        buffer.Advance(size);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write(Span<byte> buffer, in Guid value, out int size) =>
        value.TryWriteBytes(buffer, bigEndian, out size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write(ArrayBufferWriter<byte> buffer, in Guid value)
    {
        var span = buffer.ReadSpan(Unsafe.SizeOf<Guid>());

        if (!value.TryWriteBytes(span, bigEndian, out var size))
            return false;

        buffer.Advance(size);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<short> value)
    {
        if (reverse)
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<short>(buffer, value.Length));
        else
            Mem.WriteSpan(buffer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ushort> value)
    {
        if (reverse)
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<ushort>(buffer, value.Length));
        else
            Mem.WriteSpan(buffer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<int> value)
    {
        if (reverse)
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<int>(buffer, value.Length));
        else
            Mem.WriteSpan(buffer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<uint> value)
    {
        if (reverse)
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<uint>(buffer, value.Length));
        else
            Mem.WriteSpan(buffer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<long> value)
    {
        if (reverse)
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<long>(buffer, value.Length));
        else
            Mem.WriteSpan(buffer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<ulong> value)
    {
        if (reverse)
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<ulong>(buffer, value.Length));
        else
            Mem.WriteSpan(buffer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<Int128> value)
    {
        if (reverse)
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<Int128>(buffer, value.Length));
        else
            Mem.WriteSpan(buffer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<UInt128> value)
    {
        if (reverse)
            BinaryPrimitives.ReverseEndianness(value, Mem.AllocateBuffer<UInt128>(buffer, value.Length));
        else
            Mem.WriteSpan(buffer, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<short> value)
    {
        if (!reverse)
            return Mem.WriteSpan(buffer, value);

        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<short>(buffer, value.Length, out var size));
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<ushort> value)
    {
        if (!reverse)
            return Mem.WriteSpan(buffer, value);

        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<ushort>(buffer, value.Length, out var size));
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<int> value)
    {
        if (!reverse)
            return Mem.WriteSpan(buffer, value);

        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<int>(buffer, value.Length, out var size));
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<uint> value)
    {
        if (!reverse)
            return Mem.WriteSpan(buffer, value);

        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<uint>(buffer, value.Length, out var size));
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<long> value)
    {
        if (!reverse)
            return Mem.WriteSpan(buffer, value);

        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<long>(buffer, value.Length, out var size));
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<ulong> value)
    {
        if (!reverse)
            return Mem.WriteSpan(buffer, value);

        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<ulong>(buffer, value.Length, out var size));
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<Int128> value)
    {
        if (!reverse)
            return Mem.WriteSpan(buffer, value);

        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<Int128>(buffer, value.Length, out var size));
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<UInt128> value)
    {
        if (!reverse)
            return Mem.WriteSpan(buffer, value);

        BinaryPrimitives.ReverseEndianness(value, Mem.CastSpan<UInt128>(buffer, value.Length, out var size));
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<char> value) =>
        Write(buffer, MemoryMarshal.Cast<char, ushort>(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<char> value) =>
        Write(buffer, MemoryMarshal.Cast<char, ushort>(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<float> values) =>
        Write(buffer, MemoryMarshal.Cast<float, int>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<double> values) =>
        Write(buffer, MemoryMarshal.Cast<double, long>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Write(Span<byte> buffer, in ReadOnlySpan<Half> values) =>
        Write(buffer, MemoryMarshal.Cast<Half, short>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<float> values) =>
        Write(buffer, MemoryMarshal.Cast<float, int>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<double> values) =>
        Write(buffer, MemoryMarshal.Cast<double, long>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ArrayBufferWriter<byte> buffer, in ReadOnlySpan<Half> values) =>
        Write(buffer, MemoryMarshal.Cast<Half, short>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write(Span<byte> buffer, in Half value, out int size) =>
        Write(buffer, BitConverter.HalfToInt16Bits(value), out size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write(Span<byte> buffer, in float value, out int size) =>
        Write(buffer, BitConverter.SingleToInt32Bits(value), out size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write(Span<byte> buffer, in double value, out int size) =>
        Write(buffer, BitConverter.DoubleToInt64Bits(value), out size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write(ArrayBufferWriter<byte> buffer, in Half value) =>
        Write(buffer, BitConverter.HalfToInt16Bits(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write(ArrayBufferWriter<byte> buffer, in float value) =>
        Write(buffer, BitConverter.SingleToInt32Bits(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Write(ArrayBufferWriter<byte> buffer, in double value) =>
        Write(buffer, BitConverter.DoubleToInt64Bits(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<short> values)
    {
        if (!reverse) return Mem.ReadSpan(buffer, values);
        var data = Mem.CastSpan<short>(buffer, values.Length, out var size);
        BinaryPrimitives.ReverseEndianness(data, values);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<ushort> values)
    {
        if (!reverse) return Mem.ReadSpan(buffer, values);
        var data = Mem.CastSpan<ushort>(buffer, values.Length, out var size);
        BinaryPrimitives.ReverseEndianness(data, values);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<int> values)
    {
        if (!reverse) return Mem.ReadSpan(buffer, values);
        var data = Mem.CastSpan<int>(buffer, values.Length, out var size);
        BinaryPrimitives.ReverseEndianness(data, values);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<uint> values)
    {
        if (!reverse) return Mem.ReadSpan(buffer, values);
        var data = Mem.CastSpan<uint>(buffer, values.Length, out var size);
        BinaryPrimitives.ReverseEndianness(data, values);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<long> values)
    {
        if (!reverse) return Mem.ReadSpan(buffer, values);
        var data = Mem.CastSpan<long>(buffer, values.Length, out var size);
        BinaryPrimitives.ReverseEndianness(data, values);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<ulong> values)
    {
        if (!reverse) return Mem.ReadSpan(buffer, values);
        var data = Mem.CastSpan<ulong>(buffer, values.Length, out var size);
        BinaryPrimitives.ReverseEndianness(data, values);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<Int128> values)
    {
        if (!reverse) return Mem.ReadSpan(buffer, values);
        var data = Mem.CastSpan<Int128>(buffer, values.Length, out var size);
        BinaryPrimitives.ReverseEndianness(data, values);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<UInt128> values)
    {
        if (!reverse) return Mem.ReadSpan(buffer, values);
        var data = Mem.CastSpan<UInt128>(buffer, values.Length, out var size);
        BinaryPrimitives.ReverseEndianness(data, values);
        return size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<char> values) =>
        Read(buffer, MemoryMarshal.Cast<char, ushort>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<Half> values) =>
        Read(buffer, MemoryMarshal.Cast<Half, short>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<float> values) =>
        Read(buffer, MemoryMarshal.Cast<float, int>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read(ReadOnlySpan<byte> buffer, in Span<double> values) =>
        Read(buffer, MemoryMarshal.Cast<double, long>(values));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Guid ReadGuid(ReadOnlySpan<byte> buffer, out int size)
    {
        size = Unsafe.SizeOf<Guid>();
        return new(buffer[..size], bigEndian);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Half ReadHalf(ReadOnlySpan<byte> buffer, out int size)
    {
        var num = Read<short>(buffer, false, out size);
        return BitConverter.Int16BitsToHalf(num);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadFloat(ReadOnlySpan<byte> buffer, out int size)
    {
        var num = Read<int>(buffer, false, out size);
        return BitConverter.Int32BitsToSingle(num);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble(ReadOnlySpan<byte> buffer, out int size)
    {
        var num = Read<long>(buffer, false, out size);
        return BitConverter.Int64BitsToDouble(num);
    }
}
