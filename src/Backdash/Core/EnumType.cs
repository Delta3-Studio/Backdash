#pragma warning disable S2743
// ReSharper disable StaticMemberInGenericType, UnusedType.Global, UnusedMember.Global

using System.Runtime.CompilerServices;

namespace Backdash;

/// <summary>
/// Enum type helpers
/// </summary>
public static class EnumType
{
    /// <summary>Underlying type of the enumeration <typeparamref name="TEnum"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type GetUnderlyingType<TEnum>() where TEnum : unmanaged, Enum => EnumType<TEnum>.UnderlyingType;

    /// <summary>Returns the underlying enum type code of the enumeration <typeparamref name="TEnum"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EnumTypeCode GetEnumTypeCode<TEnum>() where TEnum : unmanaged, Enum => EnumType<TEnum>.TypeCode;

    /// <summary>Returns the underlying enum type code for the type <paramref name="type"/>.</summary>
    /// <returns><see langword="null"/> if the requested type is not an enum type</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EnumTypeCode? GetEnumTypeCode(Type type) =>
        type.IsEnum ? GetTypeCode(Enum.GetUnderlyingType(type)) : null;

    /// <summary>Converts the specified value to a 32-bit signed integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt32<TEnum>(TEnum value) where TEnum : unmanaged, Enum
    {
        var type = EnumType<TEnum>.UnderlyingType;

        if (type == typeof(int)
            || type == typeof(byte) || type == typeof(sbyte)
            || type == typeof(short) || type == typeof(ushort))
            return value.GetHashCode();

        if (type == typeof(uint)) return checked((int)Unsafe.As<TEnum, uint>(ref value));
        if (type == typeof(long)) return checked((int)Unsafe.As<TEnum, long>(ref value));
        if (type == typeof(ulong)) return checked((int)Unsafe.As<TEnum, ulong>(ref value));
        throw new NotSupportedException();
    }

    /// <summary>Converts the specified value to a 32-bit unsigned integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ToUInt32<TEnum>(TEnum value) where TEnum : unmanaged, Enum
    {
        var type = EnumType<TEnum>.UnderlyingType;
        if (type == typeof(int)) return checked((uint)Unsafe.As<TEnum, int>(ref value));
        if (type == typeof(byte)) return Unsafe.As<TEnum, byte>(ref value);
        if (type == typeof(ushort)) return Unsafe.As<TEnum, ushort>(ref value);
        if (type == typeof(short)) return checked((uint)Unsafe.As<TEnum, short>(ref value));
        if (type == typeof(sbyte)) return checked((uint)Unsafe.As<TEnum, sbyte>(ref value));
        if (type == typeof(uint)) return Unsafe.As<TEnum, uint>(ref value);
        if (type == typeof(long)) return checked((uint)Unsafe.As<TEnum, long>(ref value));
        if (type == typeof(ulong)) return checked((uint)Unsafe.As<TEnum, ulong>(ref value));
        throw new NotSupportedException();
    }

    /// <summary>Converts the specified value to a 64-bit signed integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToInt64<TEnum>(TEnum value) where TEnum : unmanaged, Enum
    {
        var type = EnumType<TEnum>.UnderlyingType;
        if (type == typeof(int)) return Unsafe.As<TEnum, int>(ref value);
        if (type == typeof(byte)) return Unsafe.As<TEnum, byte>(ref value);
        if (type == typeof(ushort)) return Unsafe.As<TEnum, ushort>(ref value);
        if (type == typeof(short)) return Unsafe.As<TEnum, short>(ref value);
        if (type == typeof(sbyte)) return Unsafe.As<TEnum, sbyte>(ref value);
        if (type == typeof(uint)) return Unsafe.As<TEnum, uint>(ref value);
        if (type == typeof(long)) return Unsafe.As<TEnum, long>(ref value);
        if (type == typeof(ulong)) return checked((long)Unsafe.As<TEnum, ulong>(ref value));
        throw new NotSupportedException();
    }

    /// <summary>Converts the specified value to a 64-bit unsigned integer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ToUInt64<TEnum>(TEnum value) where TEnum : unmanaged, Enum
    {
        var type = EnumType<TEnum>.UnderlyingType;
        if (type == typeof(int)) return checked((ulong)Unsafe.As<TEnum, int>(ref value));
        if (type == typeof(byte)) return Unsafe.As<TEnum, byte>(ref value);
        if (type == typeof(ushort)) return Unsafe.As<TEnum, ushort>(ref value);
        if (type == typeof(short)) return checked((ulong)Unsafe.As<TEnum, short>(ref value));
        if (type == typeof(sbyte)) return checked((ulong)Unsafe.As<TEnum, sbyte>(ref value));
        if (type == typeof(uint)) return Unsafe.As<TEnum, uint>(ref value);
        if (type == typeof(long)) return checked((ulong)Unsafe.As<TEnum, long>(ref value));
        if (type == typeof(ulong)) return Unsafe.As<TEnum, ulong>(ref value);
        throw new NotSupportedException();
    }

    /// <inheritdoc cref="Size(EnumTypeCode)"/>
    public static int GetSize<TEnum>() where TEnum : unmanaged, Enum => GetEnumTypeCode<TEnum>().Size();

    /// <inheritdoc cref="IsSigned(EnumTypeCode)"/>
    public static bool IsSigned<TEnum>() where TEnum : unmanaged, Enum => GetEnumTypeCode<TEnum>().IsSigned();

    /// <inheritdoc cref="IsUnsigned(EnumTypeCode)"/>
    public static bool IsUnsigned<TEnum>() where TEnum : unmanaged, Enum => GetEnumTypeCode<TEnum>().IsUnsigned();

    /// <inheritdoc cref="IsInt32Compatible"/>
    public static bool IsInt32Compatible<TEnum>() where TEnum : unmanaged, Enum =>
        GetEnumTypeCode<TEnum>().IsInt32Compatible();

    extension(EnumTypeCode value)
    {
        /// <summary>Returns true if the underlying type is signed.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSigned() => ((byte)value & 1) is not 0;

        /// <summary>Returns true if the underlying type is unsigned.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsUnsigned() => ((byte)value & 1) is 0;

        /// <summary>Returns the underlying type size in bytes.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Size() => 1 << ((byte)value >> 1);

        /// <summary>Returns true if the underlying type is 32-bit or smaller.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInt32Compatible() => value is EnumTypeCode.Int32 or < EnumTypeCode.UInt32;
    }

    /// <summary>Converts to <see cref="System.TypeCode"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeCode ToTypeCode(EnumTypeCode value) => value switch
    {
        EnumTypeCode.Byte => TypeCode.Byte,
        EnumTypeCode.SByte => TypeCode.SByte,
        EnumTypeCode.UInt16 => TypeCode.UInt16,
        EnumTypeCode.Int16 => TypeCode.Int16,
        EnumTypeCode.UInt32 => TypeCode.UInt32,
        EnumTypeCode.Int32 => TypeCode.Int32,
        EnumTypeCode.UInt64 => TypeCode.UInt64,
        EnumTypeCode.Int64 => TypeCode.Int64,
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static EnumTypeCode GetTypeCode(Type type)
    {
        if (type == typeof(byte)) return EnumTypeCode.Byte;
        if (type == typeof(sbyte)) return EnumTypeCode.SByte;
        if (type == typeof(short)) return EnumTypeCode.Int16;
        if (type == typeof(ushort)) return EnumTypeCode.UInt16;
        if (type == typeof(int)) return EnumTypeCode.Int32;
        if (type == typeof(uint)) return EnumTypeCode.UInt32;
        if (type == typeof(long)) return EnumTypeCode.Int64;
        if (type == typeof(ulong)) return EnumTypeCode.UInt64;
        throw new NotSupportedException();
    }

    /// <summary>Converts from <see cref="System.TypeCode"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">If the value is not a convertable <see cref="TypeCode"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EnumTypeCode FromTypeCode(TypeCode value) =>
        TryFromTypeCode(value, out var result)
            ? result
            : throw new ArgumentOutOfRangeException(nameof(value));

    /// <summary>Attempts to convert a <see cref="TypeCode"/> to an <see cref="EnumTypeCode"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFromTypeCode(TypeCode value, out EnumTypeCode result)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (value)
        {
            case TypeCode.Byte:
                result = EnumTypeCode.Byte;
                return true;
            case TypeCode.SByte:
                result = EnumTypeCode.SByte;
                return true;
            case TypeCode.UInt16:
                result = EnumTypeCode.UInt16;
                return true;
            case TypeCode.Int16:
                result = EnumTypeCode.Int16;
                return true;
            case TypeCode.UInt32:
                result = EnumTypeCode.UInt32;
                return true;
            case TypeCode.Int32:
                result = EnumTypeCode.Int32;
                return true;
            case TypeCode.UInt64:
                result = EnumTypeCode.UInt64;
                return true;
            case TypeCode.Int64:
                result = EnumTypeCode.Int64;
                return true;
            default:
                result = default;
                return false;
        }
    }
}

static class EnumType<TEnum> where TEnum : unmanaged, Enum
{
    public static readonly Type UnderlyingType = Enum.GetUnderlyingType(typeof(TEnum));
    public static readonly EnumTypeCode TypeCode = EnumType.GetTypeCode(UnderlyingType);
}

/// <summary>
///     Type representing an enum underlying type.
/// </summary>
#pragma warning disable CS1591
public enum EnumTypeCode : byte
{
    Byte = 0,
    SByte = 1,
    UInt16 = 2,
    Int16 = 3,
    UInt32 = 4,
    Int32 = 5,
    UInt64 = 6,
    Int64 = 7,
}
