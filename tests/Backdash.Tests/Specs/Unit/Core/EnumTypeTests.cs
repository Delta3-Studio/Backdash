using Backdash.Tests.TestUtils.Types;

namespace Backdash.Tests.Specs.Unit.Core;

using FluentAssertions;
using Xunit;

public class EnumTypeTests
{
    [Theory]
    [InlineData(typeof(ByteEnum), typeof(byte))]
    [InlineData(typeof(SByteEnum), typeof(sbyte))]
    [InlineData(typeof(UInt16Enum), typeof(ushort))]
    [InlineData(typeof(Int16Enum), typeof(short))]
    [InlineData(typeof(UInt32Enum), typeof(uint))]
    [InlineData(typeof(Int32Enum), typeof(int))]
    [InlineData(typeof(UInt64Enum), typeof(ulong))]
    [InlineData(typeof(Int64Enum), typeof(long))]
    public void GetUnderlyingType_ShouldReturnUnderlyingType(Type enumType, Type expected)
        => GetUnderlyingType(enumType).Should().Be(expected);

    [Theory]
    [InlineData(typeof(ByteEnum), EnumTypeCode.Byte)]
    [InlineData(typeof(SByteEnum), EnumTypeCode.SByte)]
    [InlineData(typeof(UInt16Enum), EnumTypeCode.UInt16)]
    [InlineData(typeof(Int16Enum), EnumTypeCode.Int16)]
    [InlineData(typeof(UInt32Enum), EnumTypeCode.UInt32)]
    [InlineData(typeof(Int32Enum), EnumTypeCode.Int32)]
    [InlineData(typeof(UInt64Enum), EnumTypeCode.UInt64)]
    [InlineData(typeof(Int64Enum), EnumTypeCode.Int64)]
    public void GetUnderlyingTypeCode_ShouldReturnExpectedCode(Type enumType, EnumTypeCode expected)
        => GetUnderlyingTypeCode(enumType).Should().Be(expected);

    [Fact]
    public void GetUnderlyingTypeCode_WithNonEnumType_ShouldReturnNull()
        => EnumType.GetEnumTypeCode(typeof(int)).Should().BeNull();

    [Theory]
    [InlineData(EnumTypeCode.Byte, false, true, sizeof(byte))]
    [InlineData(EnumTypeCode.SByte, true, false, sizeof(sbyte))]
    [InlineData(EnumTypeCode.UInt16, false, true, sizeof(ushort))]
    [InlineData(EnumTypeCode.Int16, true, false, sizeof(short))]
    [InlineData(EnumTypeCode.UInt32, false, true, sizeof(uint))]
    [InlineData(EnumTypeCode.Int32, true, false, sizeof(int))]
    [InlineData(EnumTypeCode.UInt64, false, true, sizeof(ulong))]
    [InlineData(EnumTypeCode.Int64, true, false, sizeof(long))]
    public void EnumTypeCode_Extensions_ShouldReturnExpectedValues(
        EnumTypeCode code, bool isSigned, bool isUnsigned, int size
    )
    {
        code.IsSigned().Should().Be(isSigned);
        code.IsUnsigned().Should().Be(isUnsigned);
        code.Size().Should().Be(size);
    }

    [Theory]
    [InlineData(EnumTypeCode.Byte, true)]
    [InlineData(EnumTypeCode.SByte, true)]
    [InlineData(EnumTypeCode.UInt16, true)]
    [InlineData(EnumTypeCode.Int16, true)]
    [InlineData(EnumTypeCode.UInt32, false)]
    [InlineData(EnumTypeCode.Int32, true)]
    [InlineData(EnumTypeCode.UInt64, false)]
    [InlineData(EnumTypeCode.Int64, false)]
    public void IsInt32Compatible_ShouldReturnExpectedValue(EnumTypeCode code, bool expected)
        => code.IsInt32Compatible().Should().Be(expected);

    [Theory]
    [InlineData(typeof(ByteEnum), EnumTypeCode.Byte, sizeof(byte), false, true)]
    [InlineData(typeof(SByteEnum), EnumTypeCode.SByte, sizeof(sbyte), true, false)]
    [InlineData(typeof(UInt16Enum), EnumTypeCode.UInt16, sizeof(ushort), false, true)]
    [InlineData(typeof(Int16Enum), EnumTypeCode.Int16, sizeof(short), true, false)]
    [InlineData(typeof(UInt32Enum), EnumTypeCode.UInt32, sizeof(uint), false, true)]
    [InlineData(typeof(Int32Enum), EnumTypeCode.Int32, sizeof(int), true, false)]
    [InlineData(typeof(UInt64Enum), EnumTypeCode.UInt64, sizeof(ulong), false, true)]
    [InlineData(typeof(Int64Enum), EnumTypeCode.Int64, sizeof(long), true, false)]
    public void GenericMetadataMethods_ShouldReturnExpectedValues(
        Type enumType, EnumTypeCode expectedCode,
        int expectedSize, bool expectedSigned, bool expectedUnsigned)
    {
        GetUnderlyingTypeCode(enumType).Should().Be(expectedCode);
        GetSize(enumType).Should().Be(expectedSize);
        IsSigned(enumType).Should().Be(expectedSigned);
        IsUnsigned(enumType).Should().Be(expectedUnsigned);
    }

    [Theory]
    [InlineData(EnumTypeCode.Byte, TypeCode.Byte)]
    [InlineData(EnumTypeCode.SByte, TypeCode.SByte)]
    [InlineData(EnumTypeCode.UInt16, TypeCode.UInt16)]
    [InlineData(EnumTypeCode.Int16, TypeCode.Int16)]
    [InlineData(EnumTypeCode.UInt32, TypeCode.UInt32)]
    [InlineData(EnumTypeCode.Int32, TypeCode.Int32)]
    [InlineData(EnumTypeCode.UInt64, TypeCode.UInt64)]
    [InlineData(EnumTypeCode.Int64, TypeCode.Int64)]
    public void ToTypeCode_ShouldReturnExpectedValue(EnumTypeCode value, TypeCode expected)
        => EnumType.ToTypeCode(value).Should().Be(expected);

    [Theory]
    [InlineData(TypeCode.Byte, EnumTypeCode.Byte)]
    [InlineData(TypeCode.SByte, EnumTypeCode.SByte)]
    [InlineData(TypeCode.UInt16, EnumTypeCode.UInt16)]
    [InlineData(TypeCode.Int16, EnumTypeCode.Int16)]
    [InlineData(TypeCode.UInt32, EnumTypeCode.UInt32)]
    [InlineData(TypeCode.Int32, EnumTypeCode.Int32)]
    [InlineData(TypeCode.UInt64, EnumTypeCode.UInt64)]
    [InlineData(TypeCode.Int64, EnumTypeCode.Int64)]
    public void FromTypeCode_ShouldReturnExpectedValue(TypeCode value, EnumTypeCode expected)
        => EnumType.FromTypeCode(value).Should().Be(expected);

    [Theory]
    [InlineData(TypeCode.Empty)]
    [InlineData(TypeCode.Object)]
    [InlineData(TypeCode.Boolean)]
    [InlineData(TypeCode.Char)]
    [InlineData(TypeCode.Decimal)]
    [InlineData(TypeCode.Single)]
    [InlineData(TypeCode.Double)]
    [InlineData(TypeCode.DateTime)]
    [InlineData(TypeCode.String)]
    public void FromTypeCode_WithUnsupportedValue_ShouldThrow(TypeCode value)
    {
        var act = () => EnumType.FromTypeCode(value);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName(nameof(value));
    }

    [Theory]
    [InlineData(TypeCode.Byte, true, EnumTypeCode.Byte)]
    [InlineData(TypeCode.SByte, true, EnumTypeCode.SByte)]
    [InlineData(TypeCode.UInt16, true, EnumTypeCode.UInt16)]
    [InlineData(TypeCode.Int16, true, EnumTypeCode.Int16)]
    [InlineData(TypeCode.UInt32, true, EnumTypeCode.UInt32)]
    [InlineData(TypeCode.Int32, true, EnumTypeCode.Int32)]
    [InlineData(TypeCode.UInt64, true, EnumTypeCode.UInt64)]
    [InlineData(TypeCode.Int64, true, EnumTypeCode.Int64)]
    [InlineData(TypeCode.Empty, false, default(EnumTypeCode))]
    [InlineData(TypeCode.Object, false, default(EnumTypeCode))]
    [InlineData(TypeCode.Boolean, false, default(EnumTypeCode))]
    [InlineData(TypeCode.Char, false, default(EnumTypeCode))]
    [InlineData(TypeCode.Decimal, false, default(EnumTypeCode))]
    [InlineData(TypeCode.Single, false, default(EnumTypeCode))]
    [InlineData(TypeCode.Double, false, default(EnumTypeCode))]
    [InlineData(TypeCode.DateTime, false, default(EnumTypeCode))]
    [InlineData(TypeCode.String, false, default(EnumTypeCode))]
    public void TryFromTypeCode_ShouldReturnExpectedResult(
        TypeCode value,
        bool expectedResult,
        EnumTypeCode expectedCode)
    {
        var result = EnumType.TryFromTypeCode(value, out var code);

        result.Should().Be(expectedResult);
        code.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData((byte)EnumTypeCode.Byte, TypeCode.Byte)]
    [InlineData((byte)EnumTypeCode.SByte, TypeCode.SByte)]
    [InlineData((byte)EnumTypeCode.UInt16, TypeCode.UInt16)]
    [InlineData((byte)EnumTypeCode.Int16, TypeCode.Int16)]
    [InlineData((byte)EnumTypeCode.UInt32, TypeCode.UInt32)]
    [InlineData((byte)EnumTypeCode.Int32, TypeCode.Int32)]
    [InlineData((byte)EnumTypeCode.UInt64, TypeCode.UInt64)]
    [InlineData((byte)EnumTypeCode.Int64, TypeCode.Int64)]
    public void TypeCodeConversions_ShouldRoundTrip(byte rawValue, TypeCode expected)
    {
        var code = (EnumTypeCode)rawValue;

        EnumType.FromTypeCode(EnumType.ToTypeCode(code)).Should().Be(code);
        EnumType.ToTypeCode(EnumType.FromTypeCode(expected)).Should().Be(expected);
    }

    static Type GetUnderlyingType(Type enumType) => enumType switch
    {
        _ when enumType == typeof(ByteEnum) => EnumType.GetUnderlyingType<ByteEnum>(),
        _ when enumType == typeof(SByteEnum) => EnumType.GetUnderlyingType<SByteEnum>(),
        _ when enumType == typeof(UInt16Enum) => EnumType.GetUnderlyingType<UInt16Enum>(),
        _ when enumType == typeof(Int16Enum) => EnumType.GetUnderlyingType<Int16Enum>(),
        _ when enumType == typeof(UInt32Enum) => EnumType.GetUnderlyingType<UInt32Enum>(),
        _ when enumType == typeof(Int32Enum) => EnumType.GetUnderlyingType<Int32Enum>(),
        _ when enumType == typeof(UInt64Enum) => EnumType.GetUnderlyingType<UInt64Enum>(),
        _ when enumType == typeof(Int64Enum) => EnumType.GetUnderlyingType<Int64Enum>(),
        _ => throw new ArgumentOutOfRangeException(nameof(enumType)),
    };

    static EnumTypeCode? GetUnderlyingTypeCode(Type enumType) => enumType switch
    {
        _ when enumType == typeof(ByteEnum) => EnumType.GetEnumTypeCode<ByteEnum>(),
        _ when enumType == typeof(SByteEnum) => EnumType.GetEnumTypeCode<SByteEnum>(),
        _ when enumType == typeof(UInt16Enum) => EnumType.GetEnumTypeCode<UInt16Enum>(),
        _ when enumType == typeof(Int16Enum) => EnumType.GetEnumTypeCode<Int16Enum>(),
        _ when enumType == typeof(UInt32Enum) => EnumType.GetEnumTypeCode<UInt32Enum>(),
        _ when enumType == typeof(Int32Enum) => EnumType.GetEnumTypeCode<Int32Enum>(),
        _ when enumType == typeof(UInt64Enum) => EnumType.GetEnumTypeCode<UInt64Enum>(),
        _ when enumType == typeof(Int64Enum) => EnumType.GetEnumTypeCode<Int64Enum>(),
        _ => null,
    };

    static int GetSize(Type enumType) => enumType switch
    {
        _ when enumType == typeof(ByteEnum) => EnumType.GetSize<ByteEnum>(),
        _ when enumType == typeof(SByteEnum) => EnumType.GetSize<SByteEnum>(),
        _ when enumType == typeof(UInt16Enum) => EnumType.GetSize<UInt16Enum>(),
        _ when enumType == typeof(Int16Enum) => EnumType.GetSize<Int16Enum>(),
        _ when enumType == typeof(UInt32Enum) => EnumType.GetSize<UInt32Enum>(),
        _ when enumType == typeof(Int32Enum) => EnumType.GetSize<Int32Enum>(),
        _ when enumType == typeof(UInt64Enum) => EnumType.GetSize<UInt64Enum>(),
        _ when enumType == typeof(Int64Enum) => EnumType.GetSize<Int64Enum>(),
        _ => throw new ArgumentOutOfRangeException(nameof(enumType)),
    };

    static bool IsSigned(Type enumType) => enumType switch
    {
        _ when enumType == typeof(ByteEnum) => EnumType.IsSigned<ByteEnum>(),
        _ when enumType == typeof(SByteEnum) => EnumType.IsSigned<SByteEnum>(),
        _ when enumType == typeof(UInt16Enum) => EnumType.IsSigned<UInt16Enum>(),
        _ when enumType == typeof(Int16Enum) => EnumType.IsSigned<Int16Enum>(),
        _ when enumType == typeof(UInt32Enum) => EnumType.IsSigned<UInt32Enum>(),
        _ when enumType == typeof(Int32Enum) => EnumType.IsSigned<Int32Enum>(),
        _ when enumType == typeof(UInt64Enum) => EnumType.IsSigned<UInt64Enum>(),
        _ when enumType == typeof(Int64Enum) => EnumType.IsSigned<Int64Enum>(),
        _ => throw new ArgumentOutOfRangeException(nameof(enumType)),
    };

    static bool IsUnsigned(Type enumType) => enumType switch
    {
        _ when enumType == typeof(ByteEnum) => EnumType.IsUnsigned<ByteEnum>(),
        _ when enumType == typeof(SByteEnum) => EnumType.IsUnsigned<SByteEnum>(),
        _ when enumType == typeof(UInt16Enum) => EnumType.IsUnsigned<UInt16Enum>(),
        _ when enumType == typeof(Int16Enum) => EnumType.IsUnsigned<Int16Enum>(),
        _ when enumType == typeof(UInt32Enum) => EnumType.IsUnsigned<UInt32Enum>(),
        _ when enumType == typeof(Int32Enum) => EnumType.IsUnsigned<Int32Enum>(),
        _ when enumType == typeof(UInt64Enum) => EnumType.IsUnsigned<UInt64Enum>(),
        _ when enumType == typeof(Int64Enum) => EnumType.IsUnsigned<Int64Enum>(),
        _ => throw new ArgumentOutOfRangeException(nameof(enumType)),
    };

    [Fact]
    public void ToInt32_ShouldConvertValuesWithinRange()
    {
        EnumType.ToInt32((ByteEnum)byte.MaxValue).Should().Be(byte.MaxValue);
        EnumType.ToInt32((SByteEnum)sbyte.MinValue).Should().Be(sbyte.MinValue);
        EnumType.ToInt32((SByteEnum)sbyte.MaxValue).Should().Be(sbyte.MaxValue);
        EnumType.ToInt32((UInt16Enum)ushort.MaxValue).Should().Be(ushort.MaxValue);
        EnumType.ToInt32((Int16Enum)short.MinValue).Should().Be(short.MinValue);
        EnumType.ToInt32((Int16Enum)short.MaxValue).Should().Be(short.MaxValue);
        EnumType.ToInt32((UInt32Enum)int.MaxValue).Should().Be(int.MaxValue);
        EnumType.ToInt32((Int32Enum)int.MinValue).Should().Be(int.MinValue);
        EnumType.ToInt32((Int32Enum)int.MaxValue).Should().Be(int.MaxValue);
        EnumType.ToInt32((UInt64Enum)int.MaxValue).Should().Be(int.MaxValue);
        EnumType.ToInt32((Int64Enum)int.MinValue).Should().Be(int.MinValue);
        EnumType.ToInt32((Int64Enum)int.MaxValue).Should().Be(int.MaxValue);
    }

    [Fact]
    public void ToInt32_ShouldThrowWhenValueIsOutOfRange()
    {
        var uintOverflow = () => EnumType.ToInt32((UInt32Enum)((uint)int.MaxValue + 1));
        var ulongOverflow = () => EnumType.ToInt32((UInt64Enum)((ulong)int.MaxValue + 1));
        var longUnderflow = () => EnumType.ToInt32((Int64Enum)((long)int.MinValue - 1));
        var longOverflow = () => EnumType.ToInt32((Int64Enum)((long)int.MaxValue + 1));

        uintOverflow.Should().Throw<OverflowException>();
        ulongOverflow.Should().Throw<OverflowException>();
        longUnderflow.Should().Throw<OverflowException>();
        longOverflow.Should().Throw<OverflowException>();
    }

    [Fact]
    public void ToUInt32_ShouldConvertValuesWithinRange()
    {
        EnumType.ToUInt32((ByteEnum)byte.MaxValue).Should().Be(byte.MaxValue);
        EnumType.ToUInt32((SByteEnum)sbyte.MaxValue).Should().Be((uint)sbyte.MaxValue);
        EnumType.ToUInt32((UInt16Enum)ushort.MaxValue).Should().Be(ushort.MaxValue);
        EnumType.ToUInt32((Int16Enum)short.MaxValue).Should().Be((uint)short.MaxValue);
        EnumType.ToUInt32((UInt32Enum)uint.MaxValue).Should().Be(uint.MaxValue);
        EnumType.ToUInt32((Int32Enum)int.MaxValue).Should().Be(int.MaxValue);
        EnumType.ToUInt32((UInt64Enum)uint.MaxValue).Should().Be(uint.MaxValue);
        EnumType.ToUInt32((Int64Enum)uint.MaxValue).Should().Be(uint.MaxValue);
    }

    [Fact]
    public void ToUInt32_ShouldThrowWhenValueIsOutOfRange()
    {
        var sbyteUnderflow = () => EnumType.ToUInt32((SByteEnum)sbyte.MinValue);
        var int16Underflow = () => EnumType.ToUInt32((Int16Enum)short.MinValue);
        var int32Underflow = () => EnumType.ToUInt32((Int32Enum)int.MinValue);
        var int64Underflow = () => EnumType.ToUInt32((Int64Enum)long.MinValue);
        var ulongOverflow = () => EnumType.ToUInt32((UInt64Enum)((ulong)uint.MaxValue + 1));
        var longOverflow = () => EnumType.ToUInt32((Int64Enum)((long)uint.MaxValue + 1));

        sbyteUnderflow.Should().Throw<OverflowException>();
        int16Underflow.Should().Throw<OverflowException>();
        int32Underflow.Should().Throw<OverflowException>();
        int64Underflow.Should().Throw<OverflowException>();
        ulongOverflow.Should().Throw<OverflowException>();
        longOverflow.Should().Throw<OverflowException>();
    }

    [Fact]
    public void ToInt64_ShouldConvertValuesWithinRange()
    {
        EnumType.ToInt64((ByteEnum)byte.MaxValue).Should().Be(byte.MaxValue);
        EnumType.ToInt64((SByteEnum)sbyte.MinValue).Should().Be(sbyte.MinValue);
        EnumType.ToInt64((SByteEnum)sbyte.MaxValue).Should().Be(sbyte.MaxValue);
        EnumType.ToInt64((UInt16Enum)ushort.MaxValue).Should().Be(ushort.MaxValue);
        EnumType.ToInt64((Int16Enum)short.MinValue).Should().Be(short.MinValue);
        EnumType.ToInt64((Int16Enum)short.MaxValue).Should().Be(short.MaxValue);
        EnumType.ToInt64((UInt32Enum)uint.MaxValue).Should().Be(uint.MaxValue);
        EnumType.ToInt64((Int32Enum)int.MinValue).Should().Be(int.MinValue);
        EnumType.ToInt64((Int32Enum)int.MaxValue).Should().Be(int.MaxValue);
        EnumType.ToInt64((UInt64Enum)long.MaxValue).Should().Be(long.MaxValue);
        EnumType.ToInt64((Int64Enum)long.MinValue).Should().Be(long.MinValue);
        EnumType.ToInt64((Int64Enum)long.MaxValue).Should().Be(long.MaxValue);
    }

    [Fact]
    public void ToInt64_ShouldThrowWhenValueIsOutOfRange()
    {
        var ulongOverflow = () => EnumType.ToInt64((UInt64Enum)((ulong)long.MaxValue + 1));
        var ulongMaximum = () => EnumType.ToInt64((UInt64Enum)ulong.MaxValue);
        ulongOverflow.Should().Throw<OverflowException>();
        ulongMaximum.Should().Throw<OverflowException>();
    }

    [Fact]
    public void ToUInt64_ShouldConvertValuesWithinRange()
    {
        EnumType.ToUInt64((ByteEnum)byte.MaxValue).Should().Be(byte.MaxValue);
        EnumType.ToUInt64((SByteEnum)sbyte.MaxValue).Should().Be((ulong)sbyte.MaxValue);
        EnumType.ToUInt64((UInt16Enum)ushort.MaxValue).Should().Be(ushort.MaxValue);
        EnumType.ToUInt64((Int16Enum)short.MaxValue).Should().Be((ulong)short.MaxValue);
        EnumType.ToUInt64((UInt32Enum)uint.MaxValue).Should().Be(uint.MaxValue);
        EnumType.ToUInt64((Int32Enum)int.MaxValue).Should().Be(int.MaxValue);
        EnumType.ToUInt64((UInt64Enum)ulong.MaxValue).Should().Be(ulong.MaxValue);
        EnumType.ToUInt64((Int64Enum)long.MaxValue).Should().Be(long.MaxValue);
    }

    [Fact]
    public void ToUInt64_ShouldThrowWhenValueIsNegative()
    {
        var sbyteUnderflow = () => EnumType.ToUInt64((SByteEnum)sbyte.MinValue);
        var int16Underflow = () => EnumType.ToUInt64((Int16Enum)short.MinValue);
        var int32Underflow = () => EnumType.ToUInt64((Int32Enum)int.MinValue);
        var int64Underflow = () => EnumType.ToUInt64((Int64Enum)long.MinValue);

        sbyteUnderflow.Should().Throw<OverflowException>();
        int16Underflow.Should().Throw<OverflowException>();
        int32Underflow.Should().Throw<OverflowException>();
        int64Underflow.Should().Throw<OverflowException>();
    }
}
