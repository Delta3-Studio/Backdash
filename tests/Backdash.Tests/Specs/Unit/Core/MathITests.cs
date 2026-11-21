using Backdash.Tests.TestUtils;
using Backdash.Tests.TestUtils.Types;

namespace Backdash.Tests.Specs.Unit.Core;

public class MathITests
{
    [PropertyTest]
    public bool ShouldSumArrayOfInt(int[] values)
    {
        var sut = MathI.Sum(values);
        var native = values.Sum();
        return sut == native;
    }

    [PropertyTest]
    public bool ShouldSumArrayOfUInt(uint[] values)
    {
        var sut = MathI.Sum(values);
        var native = values.Aggregate<uint, uint>(0, (current, value) => current + value);
        return sut == native;
    }

    [PropertyTest]
    public bool ShouldRawSumArrayOfInt(int[] values)
    {
        var sut = MathI.SumSimple(values);
        var native = values.Sum();
        return sut == native;
    }

    [PropertyTest]
    public bool ShouldRawSumArrayOfUInt(uint[] values)
    {
        var sut = MathI.SumSimple(values);
        var native = values.Aggregate<uint, uint>(0, (acc, value) => acc + value);
        return sut == native;
    }

    const double Tolerance = 0.000001;

    [PropertyTest]
    public bool ShouldAvgArrayOfInt(NonEmptyArray<int> arg)
    {
        var values = arg.Item;
        var sut = MathI.Avg(values);
        var native = values.Average();
        return Math.Abs(sut - native) < Tolerance;
    }

    [Fact]
    public void IsUnsignedReturnExpectedValue()
    {
        MathI.IsUnsigned<byte>().Should().BeTrue();
        MathI.IsUnsigned<sbyte>().Should().BeFalse();
        MathI.IsUnsigned<ushort>().Should().BeTrue();
        MathI.IsUnsigned<short>().Should().BeFalse();
        MathI.IsUnsigned<uint>().Should().BeTrue();
        MathI.IsUnsigned<int>().Should().BeFalse();
        MathI.IsUnsigned<ulong>().Should().BeTrue();
        MathI.IsUnsigned<long>().Should().BeFalse();
        MathI.IsUnsigned<UInt128>().Should().BeTrue();
        MathI.IsUnsigned<Int128>().Should().BeFalse();
        MathI.IsUnsigned<nuint>().Should().BeTrue();
        MathI.IsUnsigned<nint>().Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(ulong), true)]
    [InlineData(typeof(UInt128), true)]
    [InlineData(typeof(nuint), true)]
    [InlineData(typeof(sbyte), false)]
    [InlineData(typeof(short), false)]
    [InlineData(typeof(int), false)]
    [InlineData(typeof(long), false)]
    [InlineData(typeof(Int128), false)]
    [InlineData(typeof(nint), false)]
    [InlineData(typeof(float), false)]
    [InlineData(typeof(double), false)]
    [InlineData(typeof(decimal), false)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(char), false)]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(ByteEnum), true)]
    [InlineData(typeof(SByteEnum), false)]
    [InlineData(typeof(UInt16Enum), true)]
    [InlineData(typeof(Int16Enum), false)]
    [InlineData(typeof(UInt32Enum), true)]
    [InlineData(typeof(Int32Enum), false)]
    [InlineData(typeof(UInt64Enum), true)]
    [InlineData(typeof(Int64Enum), false)]
    public void IsUnsignedByTypeShouldReturnExpectedValue(Type type, bool expected) =>
        MathI.IsUnsigned(type).Should().Be(expected);

    [Theory]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(Int128), true)]
    [InlineData(typeof(nint), true)]
    [InlineData(typeof(byte), false)]
    [InlineData(typeof(ushort), false)]
    [InlineData(typeof(uint), false)]
    [InlineData(typeof(ulong), false)]
    [InlineData(typeof(UInt128), false)]
    [InlineData(typeof(nuint), false)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(char), false)]
    [InlineData(typeof(object), false)]
    [InlineData(typeof(ByteEnum), false)]
    [InlineData(typeof(SByteEnum), true)]
    [InlineData(typeof(UInt16Enum), false)]
    [InlineData(typeof(Int16Enum), true)]
    [InlineData(typeof(UInt32Enum), false)]
    [InlineData(typeof(Int32Enum), true)]
    [InlineData(typeof(UInt64Enum), false)]
    [InlineData(typeof(Int64Enum), true)]
    public void IsSignedByTypeShouldReturnExpectedValue(Type type, bool expected) =>
        MathI.IsSigned(type).Should().Be(expected);
}
