// ReSharper disable UnusedType.Global, UnusedMember.Global

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Backdash.Serialization;

#pragma warning disable CS1591

/// <summary>Signed number serialization extensions</summary>
public static class SerializationSignedExtensions
{
    const bool IsUnsigned = false;

    extension(ref readonly EndiannessNumberSerializer @this)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(ReadOnlySpan<byte> buffer, out int bytesRead)
            where T : unmanaged, ISignedNumber<T>, IBinaryInteger<T> =>
            @this.Read<T>(buffer, IsUnsigned, out bytesRead);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(ref T value, ReadOnlySpan<byte> buffer, out int size)
            where T : unmanaged, ISignedNumber<T>, IBinaryInteger<T> =>
            @this.Read(ref value, buffer, IsUnsigned, out size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(ref T value, ReadOnlySpan<byte> buffer, out int size)
            where T : unmanaged, ISignedNumber<T>, IBinaryInteger<T> =>
            @this.TryRead(ref value, buffer, IsUnsigned, out size);
    }

    extension(ref readonly BinaryBufferReader @this)
    {
        /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}(bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadNumber<T>() where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
            @this.ReadNumber<T>(IsUnsigned);

        /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}(ref T, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadNumber<T>(ref T value) where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
            @this.ReadNumber(ref value, IsUnsigned);

        /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}( ref T?, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadNumber<T>(ref T? value) where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
            @this.ReadNumber(ref value, IsUnsigned);

        /// <inheritdoc cref="BinaryBufferReader.ReadNullableNumber{T}(bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadNullableNumber<T>() where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
            @this.ReadNullableNumber<T>(IsUnsigned);
    }
}

/// <summary>Unsigned number serialization extensions</summary>
public static class SerializationUnsignedExtensions
{
    const bool IsUnsigned = true;

    extension(ref readonly EndiannessNumberSerializer @this)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(ReadOnlySpan<byte> buffer, out int bytesRead)
            where T : unmanaged, IUnsignedNumber<T>, IBinaryInteger<T> =>
            @this.Read<T>(buffer, IsUnsigned, out bytesRead);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T>(ref T value, ReadOnlySpan<byte> buffer, out int size)
            where T : unmanaged, IUnsignedNumber<T>, IBinaryInteger<T> =>
            @this.Read(ref value, buffer, IsUnsigned, out size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryRead<T>(ref T value, ReadOnlySpan<byte> buffer, out int size)
            where T : unmanaged, IUnsignedNumber<T>, IBinaryInteger<T> =>
            @this.TryRead(ref value, buffer, IsUnsigned, out size);
    }

    extension(ref readonly BinaryBufferReader @this)
    {
        /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}(bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadNumber<T>() where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
            @this.ReadNumber<T>(IsUnsigned);

        /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}(ref T, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadNumber<T>(ref T value) where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
            @this.ReadNumber(ref value, IsUnsigned);

        /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}( ref T?, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadNumber<T>(ref T? value) where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
            @this.ReadNumber(ref value, IsUnsigned);

        /// <inheritdoc cref="BinaryBufferReader.ReadNullableNumber{T}(bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? ReadNullableNumber<T>() where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
            @this.ReadNullableNumber<T>(IsUnsigned);
    }
}
