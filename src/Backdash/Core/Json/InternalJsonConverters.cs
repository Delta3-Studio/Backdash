using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backdash.Data;

namespace Backdash.Core;

static class JsonAttrMessages
{
    public const string UnreferencedCode =
        "JSON serialization and deserialization might require types that cannot be statically analyzed. Use the overload that takes a JsonTypeInfo instead.";

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

sealed class CircularBufferJsonConverter<T> : JsonConverter<CircularBuffer<T>>
{
    public override CircularBuffer<T>? Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var array = JsonSerializer.Deserialize<T[]>(ref reader, options);
        return array is null ? null : CircularBuffer<T>.CreateFrom(array);
    }

    public override void Write(Utf8JsonWriter writer, CircularBuffer<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.ToArray(), options);
}
