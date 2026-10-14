using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Backdash.Data;

namespace Backdash.Core.Json;

static class JsonAttrMessages
{
    public const string DynamicCode =
        "JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.";
}

sealed class UnsafeInt32JsonConverter<T> : JsonConverter<T> where T : unmanaged
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Unsafe.BitCast<int, T>(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(Unsafe.As<T, int>(ref value));
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
sealed class UnsafeInt32JsonConverterAttribute<T>()
    : JsonConverterAttribute(typeof(UnsafeInt32JsonConverter<T>))
    where T : unmanaged;

sealed class UnsafeInt64JsonConverter<T> : JsonConverter<T> where T : unmanaged
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Unsafe.BitCast<long, T>(reader.GetInt64());

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(Unsafe.As<T, long>(ref value));
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
sealed class UnsafeInt64JsonConverterAttribute<T>()
    : JsonConverterAttribute(typeof(UnsafeInt64JsonConverter<T>))
    where T : unmanaged;

/// <summary>
/// <see cref="CircularBuffer{T}"/> JSON converter.
/// </summary>
public sealed class CircularBufferJsonConverter<T> : JsonConverter<CircularBuffer<T>>
{
    /// <inheritdoc />
    public override CircularBuffer<T>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        if (reader.TokenType is not JsonTokenType.StartArray)
            throw new JsonException("Expected a JSON array.");

        if (options.GetTypeInfo(typeof(T)) is not JsonTypeInfo<T> typeInfo)
            throw new JsonException("Invalid type info.");

        List<T> items = [];

        while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray)
        {
            var item = JsonSerializer.Deserialize(ref reader, typeInfo)!;
            items.Add(item);
        }

        if (reader.TokenType is not JsonTokenType.EndArray)
            throw new JsonException("Incomplete JSON array.");

        return CircularBuffer<T>.CreateFrom([.. items]);
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        CircularBuffer<T> value,
        JsonSerializerOptions options)
    {
        if (options.GetTypeInfo(typeof(T)) is not JsonTypeInfo<T> typeInfo)
            throw new JsonException("Invalid type info.");

        writer.WriteStartArray();

        foreach (var item in value)
            JsonSerializer.Serialize(writer, item, typeInfo);

        writer.WriteEndArray();
    }
}

sealed class CircularBufferJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
        && typeToConvert.GetGenericTypeDefinition() == typeof(CircularBuffer<>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
            throw new InvalidOperationException(JsonAttrMessages.DynamicCode);

        var argType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(CircularBufferJsonConverter<>).MakeGenericType(argType);
        return Activator.CreateInstance(converterType) as JsonConverter;
    }
}
