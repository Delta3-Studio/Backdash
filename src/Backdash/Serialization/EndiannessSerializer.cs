using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
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

        public bool Write<T>(ArrayBufferWriter<byte> buffer, T value, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            size = Unsafe.SizeOf<T>();
            return value.TryWriteBigEndian(buffer.ReadSpan(size), out size);
        }

        public bool Write(Span<byte> buffer, in Guid value, out int size) =>
            value.TryWriteBytes(buffer, bigEndian: true, out size);

        public bool Write(ArrayBufferWriter<byte> buffer, in Guid value, out int size)
        {
            var span = buffer.ReadSpan(Unsafe.SizeOf<Guid>());
            return value.TryWriteBytes(span, bigEndian: true, out size);
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

        public bool Write<T>(ArrayBufferWriter<byte> buffer, T value, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            size = Unsafe.SizeOf<T>();
            return value.TryWriteLittleEndian(buffer.ReadSpan(size), out size);
        }

        public bool Write(Span<byte> buffer, in Guid value, out int size) =>
            value.TryWriteBytes(buffer, bigEndian: false, out size);

        public bool Write(ArrayBufferWriter<byte> buffer, in Guid value, out int size)
        {
            var span = buffer.ReadSpan(Unsafe.SizeOf<Guid>());
            return value.TryWriteBytes(span, bigEndian: false, out size);
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
    bool Write<T>(ArrayBufferWriter<byte> buffer, T value, out int size)
        where T : unmanaged, IBinaryInteger<T>;

    /// <summary>Write a <see cref="Guid"/> into an array buffer</summary>
    bool Write(Span<byte> buffer, in Guid value, out int size);

    /// <summary>Write a <see cref="Guid"/> value into an array buffer</summary>
    bool Write(ArrayBufferWriter<byte> buffer, in Guid value, out int size);

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
