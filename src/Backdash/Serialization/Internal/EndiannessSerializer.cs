using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using Backdash.Network;

namespace Backdash.Serialization.Internal;

/// <summary>
/// Get endianness specific number serializer
/// </summary>
public static class EndiannessSerializer
{
    /// <summary>
    /// Get endianness serializer singleton
    /// </summary>
    public static INumberSerializer Get(Endianness endianness) => endianness switch
    {
        Endianness.LittleEndian => LittleEndian.Instance,
        Endianness.BigEndian => BigEndian.Instance,
        _ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
    };

    internal class BigEndian : INumberSerializer
    {
        public static readonly BigEndian Instance = new();
        const Endianness BaseEndianness = Endianness.BigEndian;

        public Endianness Endianness { get; } = BaseEndianness;

        public bool NeedsReverse { get; } = Platform.Endianness != BaseEndianness;

        public T Read<T>(ReadOnlySpan<byte> buffer, bool isUnsigned, out int bytesRead)
            where T : unmanaged, IBinaryInteger<T>
        {
            bytesRead = Unsafe.SizeOf<T>();
            return T.ReadBigEndian(buffer[..bytesRead], isUnsigned);
        }

        public bool Write<T>(Span<byte> buffer, in T value, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            ref var valueRef = ref Unsafe.AsRef(in value);
            return valueRef.TryWriteBigEndian(buffer, out size);
        }

        public bool Write<T>(ArrayBufferWriter<byte> buffer, in T value, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            size = Unsafe.SizeOf<T>();
            ref var valueRef = ref Unsafe.AsRef(in value);
            return valueRef.TryWriteBigEndian(buffer.GetSpan(size), out size);
        }
    }

    internal class LittleEndian : INumberSerializer
    {
        public static readonly LittleEndian Instance = new();
        const Endianness BaseEndianness = Endianness.LittleEndian;
        public Endianness Endianness { get; } = BaseEndianness;

        public bool NeedsReverse { get; } = Platform.Endianness != BaseEndianness;

        public T Read<T>(ReadOnlySpan<byte> buffer, bool isUnsigned, out int bytesRead)
            where T : unmanaged, IBinaryInteger<T>
        {
            bytesRead = Unsafe.SizeOf<T>();
            return T.ReadLittleEndian(buffer[..bytesRead], isUnsigned);
        }

        public bool Write<T>(Span<byte> buffer, in T value, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            ref var valueRef = ref Unsafe.AsRef(in value);
            return valueRef.TryWriteLittleEndian(buffer, out size);
        }

        public bool Write<T>(ArrayBufferWriter<byte> buffer, in T value, out int size)
            where T : unmanaged, IBinaryInteger<T>
        {
            size = Unsafe.SizeOf<T>();
            ref var valueRef = ref Unsafe.AsRef(in value);
            return valueRef.TryWriteLittleEndian(buffer.GetSpan(size), out size);
        }
    }

    /// <summary>
    ///  Endianness specific number serializer
    /// </summary>
    public interface INumberSerializer
    {
        /// <summary>Serializer endianness</summary>
        Endianness Endianness { get; }

        /// <summary>True if it will be necessary to reverse the endianness</summary>
        bool NeedsReverse { get; }

        /// <summary>Write a number into a buffer</summary>
        bool Write<T>(Span<byte> buffer, in T value, out int size) where T : unmanaged, IBinaryInteger<T>;

        /// <summary>Write a number into an array buffer</summary>
        bool Write<T>(ArrayBufferWriter<byte> buffer, in T value, out int size)
            where T : unmanaged, IBinaryInteger<T>;

        /// <summary>Reads number from the buffer</summary>
        T Read<T>(ReadOnlySpan<byte> buffer, bool isUnsigned, out int bytesRead) where T : unmanaged, IBinaryInteger<T>;

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
    }
}
