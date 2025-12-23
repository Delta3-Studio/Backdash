using System.Numerics;
using System.Runtime.CompilerServices;

namespace Backdash.Serialization;

/// <summary>
/// Signed number extensions
/// </summary>
public static class BinarySerializerSignedExtensions
{
    const bool IsUnsigned = false;

    /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}(bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadNumber<T>(ref readonly this BinaryBufferReader reader)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber<T>(IsUnsigned);

    /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}(ref T, bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadNumber<T>(ref readonly this BinaryBufferReader reader, ref T value)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber(ref value, IsUnsigned);

    /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}( ref T?, bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadNumber<T>(ref readonly this BinaryBufferReader reader, ref T? value)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber(ref value, IsUnsigned);

    /// <inheritdoc cref="BinaryBufferReader.ReadNullableNumber{T}(bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ReadNullableNumber<T>(ref readonly this BinaryBufferReader reader)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNullableNumber<T>(IsUnsigned);
}

/// <summary>
/// Unsigned number extensions
/// </summary>
public static class BinarySerializerUnsignedExtensions
{
    const bool IsUnsigned = true;

    /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}(bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadNumber<T>(ref readonly this BinaryBufferReader reader)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber<T>(IsUnsigned);

    /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}(ref T, bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadNumber<T>(ref readonly this BinaryBufferReader reader, ref T value)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber(ref value, IsUnsigned);

    /// <inheritdoc cref="BinaryBufferReader.ReadNumber{T}( ref T?, bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadNumber<T>(ref readonly this BinaryBufferReader reader, ref T? value)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber(ref value, IsUnsigned);

    /// <inheritdoc cref="BinaryBufferReader.ReadNullableNumber{T}(bool)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ReadNullableNumber<T>(ref readonly this BinaryBufferReader reader)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNullableNumber<T>(IsUnsigned);
}
