using System.Text.Json;
using Backdash.Data;
using Backdash.Tests.TestUtils;

namespace Backdash.Tests.Specs.Unit.Data;

using static JsonSerializer;

public class FrameJsonSerializationTests() :
    BaseJsonConverterTests<Frame>(Gen.Frame, x => $"{x.Number}")
{
    [Fact] public void ShouldDeserialize() => DeserializeTest();
    [Fact] public void ShouldSerialize() => SerializeTest();
}

public class FrameSpanJsonSerializationTests() :
    BaseJsonConverterTests<FrameSpan>(Gen.FrameSpan, x => $"{x.Frames}")
{
    [Fact] public void ShouldDeserialize() => DeserializeTest();
    [Fact] public void ShouldSerialize() => SerializeTest();
}

public class FrameRangeJsonSerializationTests() :
    BaseJsonConverterTests<FrameRange>(Gen.FrameRange, x =>
        $"[{x.Start.Number},{x.End.Number}]")
{
    [Fact] public void ShouldDeserialize() => DeserializeTest();
    [Fact] public void ShouldSerialize() => SerializeTest();
}

public class ByteSizeJsonSerializationTests() :
    BaseJsonConverterTests<ByteSize>(Gen.ByteSize, x => $"{x.ByteCount}")
{
    [Fact] public void ShouldDeserialize() => DeserializeTest();
    [Fact] public void ShouldSerialize() => SerializeTest();
}

public class CircularBufferJsonSerializationTests() :
    BaseJsonConverterTests<CircularBuffer<int>>(
        () => CircularBuffer<int>.CreateFrom([10, 99, 22, 11, 77]),
        _ => "[10,99,22,11,77]"
    )
{
    [Fact] public void ShouldDeserialize() => DeserializeTest();
    [Fact] public void ShouldSerialize() => SerializeTest();
}

public abstract class BaseJsonConverterTests<T>(Func<T> getValue, Func<T, string>? getExpected = null)
{
    readonly Func<T, string> getExpected = getExpected ?? (x => x?.ToString() ?? "null");

    record TestType(T Data);

    public void DeserializeTest()
    {
        var value = getValue();
        var expected = getExpected(value);
        var result = Deserialize<TestType>($$"""{"Data":{{expected}}}""");
        result!.Data.Should().Be(value);
    }

    public void SerializeTest()
    {
        var value = getValue();
        var expected = getExpected(value);
        var json = Serialize(new TestType(value));
        var result = $$"""{"Data":{{expected}}}""";
        json.Should().Be(result);
    }
}
