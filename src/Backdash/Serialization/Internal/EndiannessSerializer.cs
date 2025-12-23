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
    ///  Endianness specific number serializer
    /// </summary>
    public interface INumberSerializer
    {
        /// <summary>Serializer endianness</summary>
        Endianness Endianness { get; }

        /// <summary>Reads number from the buffer</summary>
        T Read<T>(ReadOnlySpan<byte> buffer, bool isUnsigned, out int bytesRead) where T : unmanaged, IBinaryInteger<T>;

        /// <summary>Write a number into a buffer</summary>
        bool Write<T>(Span<byte> buffer, in T value, out int size) where T : unmanaged, IBinaryInteger<T>;

        /// <summary>Write a number into an array buffer</summary>
        bool Write<T>(ArrayBufferWriter<byte> buffer, in T value, out int size)
            where T : unmanaged, IBinaryInteger<T>;
    }

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

        public Endianness Endianness { get; } = Endianness.BigEndian;

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
        public Endianness Endianness { get; } = Endianness.LittleEndian;

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
}
