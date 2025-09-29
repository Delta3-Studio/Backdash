using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backdash.Data;

namespace Backdash.Core;

sealed class UnsafeInt32JsonConverter<T> : JsonConverter<T> where T : unmanaged
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Unsafe.BitCast<int, T>(reader.GetInt32());

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(Unsafe.As<T, int>(ref Unsafe.AsRef(ref value)));
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
        writer.WriteNumberValue(Unsafe.As<T, long>(ref Unsafe.AsRef(ref value)));
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field)]
sealed class UnsafeInt64JsonConverterAttribute<T>()
    : JsonConverterAttribute(typeof(UnsafeInt64JsonConverter<T>))
    where T : unmanaged;

sealed class CircularBufferJsonConverter<T> : JsonConverter<CircularBuffer<T>>
{
    public override CircularBuffer<T>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        var array = JsonSerializer.Deserialize<T[]>(ref reader, options);
        return array is null ? null : CircularBuffer<T>.CreateFrom(array);
    }

    public override void Write(Utf8JsonWriter writer, CircularBuffer<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.ToArray(), options);
}

sealed class CircularBufferJsonConverter : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType
        && typeToConvert.GetGenericTypeDefinition() == typeof(CircularBuffer<>);

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var argType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(CircularBufferJsonConverter<>).MakeGenericType(argType);
        return Activator.CreateInstance(converterType) as JsonConverter;
    }
}
