using System.Numerics;
using System.Runtime.CompilerServices;

namespace Backdash.Serialization;

/// <summary>
/// Signed number extensions
/// </summary>
public static class BinarySerializerSignedExtensions
{
    /// <inheritdoc cref="BinarySerializerSignedExtensions.ReadNullableNumber"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadNumber<T>(ref readonly this BinaryBufferReader reader)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber<T>(false);

    /// <inheritdoc cref="BinarySerializerSignedExtensions.ReadNullableNumber"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadNumber<T>(ref readonly this BinaryBufferReader reader, ref T value)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber(ref value, false);

    /// <inheritdoc cref="BinarySerializerSignedExtensions.ReadNullableNumber"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadNumber<T>(ref readonly this BinaryBufferReader reader, ref T? value)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNumber(ref value, false);

    /// <summary>Reads single <see cref="IBinaryInteger{T}" /> from buffer.</summary>
    /// <typeparam name="T">A numeric type that implements <see cref="IBinaryInteger{T}" />.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ReadNullableNumber<T>(ref readonly this BinaryBufferReader reader)
        where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T> =>
        reader.ReadNullableNumber<T>(false);
}

/// <summary>
/// Unsigned number extensions
/// </summary>
public static class BinarySerializerUnsignedExtensions
{
    /// <inheritdoc cref="BinarySerializerUnsignedExtensions.ReadNullableNumber"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ReadNumber<T>(ref readonly this BinaryBufferReader reader)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
        reader.ReadNumber<T>(true);

    /// <inheritdoc cref="BinarySerializerUnsignedExtensions.ReadNullableNumber"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadNumber<T>(ref readonly this BinaryBufferReader reader, ref T value)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
        reader.ReadNumber(ref value, true);

    /// <inheritdoc cref="BinarySerializerUnsignedExtensions.ReadNullableNumber"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadNumber<T>(ref readonly this BinaryBufferReader reader, ref T? value)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
        reader.ReadNumber(ref value, true);

    /// <summary>Reads single <see cref="IBinaryInteger{T}" /> from buffer.</summary>
    /// <typeparam name="T">A numeric type that implements <see cref="IBinaryInteger{T}" />.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ReadNullableNumber<T>(ref readonly this BinaryBufferReader reader)
        where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T> =>
        reader.ReadNullableNumber<T>(true);
}
