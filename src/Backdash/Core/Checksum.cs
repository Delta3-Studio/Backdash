using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backdash.Core;

namespace Backdash;

/// <summary>
///     Value representation of a Checksum
/// </summary>
[Serializable]
[DebuggerDisplay("{ToString()}")]
[JsonConverter(typeof(JsonConverter))]
public readonly record struct Checksum :
    IComparable<Checksum>,
    IComparable<uint>,
    IEquatable<uint>,
    IUtf8SpanFormattable,
    ISpanFormattable,
    IComparisonOperators<Checksum, Checksum, bool>,
    IComparisonOperators<Checksum, uint, bool>
{
    /// <summary>Return frame value <c>0</c></summary>
    public static readonly Checksum Empty = new(0);

    /// <summary>Returns the <see cref="uint" /> value for the current <see cref="Checksum" />.</summary>
    public readonly uint Value;

    /// <summary>
    ///     Initialize new <see cref="Checksum" /> for frame <paramref name="value" />.
    /// </summary>
    /// <param name="value"></param>
    public Checksum(uint value) => Value = value;

    /// <inheritdoc />
    public int CompareTo(Checksum other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public int CompareTo(uint other) => Value.CompareTo(other);

    /// <inheritdoc />
    public bool Equals(uint other) => Value == other;

    const string DefaultFormat = "x8";

    /// <inheritdoc />
    public string ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        string? format,
        IFormatProvider? formatProvider
    ) =>
        Value.ToString(format ?? DefaultFormat, formatProvider);

    /// <inheritdoc />
    public override string ToString() => ToString(null, null);

    /// <inheritdoc cref="ToString(string, IFormatProvider)" />
    public string ToString(
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        string format) => ToString(format, null);

    /// <inheritdoc />
    public bool TryFormat(
        Span<byte> utf8Destination, out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    )
    {
        bytesWritten = 0;
        if (format.IsEmpty) format = DefaultFormat;
        Utf8StringBuilder writer = new(in utf8Destination, ref bytesWritten);
        return writer.Write(Value, format);
    }

    /// <inheritdoc />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (format.IsEmpty) format = DefaultFormat;
        return Value.TryFormat(destination, out charsWritten, format, provider);
    }

    /// <inheritdoc />
    public static bool operator >(Checksum left, Checksum right) => left.Value > right.Value;

    /// <inheritdoc />
    public static bool operator >=(Checksum left, Checksum right) => left.Value >= right.Value;

    /// <inheritdoc />
    public static bool operator <(Checksum left, Checksum right) => left.Value < right.Value;

    /// <inheritdoc />
    public static bool operator <=(Checksum left, Checksum right) => left.Value <= right.Value;

    /// <inheritdoc />
    public static bool operator ==(Checksum left, uint right) => left.Value == right;

    /// <inheritdoc />
    public static bool operator !=(Checksum left, uint right) => left.Value != right;

    /// <inheritdoc />
    public static bool operator >(Checksum left, uint right) => left.Value > right;

    /// <inheritdoc />
    public static bool operator >=(Checksum left, uint right) => left.Value >= right;

    /// <inheritdoc />
    public static bool operator <(Checksum left, uint right) => left.Value < right;

    /// <inheritdoc />
    public static bool operator <=(Checksum left, uint right) => left.Value <= right;

    /// <inheritdoc cref="Value" />
    public static implicit operator uint(Checksum frame) => frame.Value;

    /// <inheritdoc cref="Checksum(uint)" />
    public static explicit operator Checksum(uint frame) => new(frame);

    internal sealed class JsonConverter : JsonConverter<Checksum>
    {
        public override Checksum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return string.IsNullOrWhiteSpace(value) ? Empty : new(uint.Parse(value, NumberStyles.HexNumber, null));
        }

        public override void Write(Utf8JsonWriter writer, Checksum value, JsonSerializerOptions options)
        {
            Span<char> buffer = stackalloc char[8];
            if (value.TryFormat(buffer, out var charCount, DefaultFormat, null))
            {
                writer.WriteStringValue(buffer[..charCount]);
            }
            else
                writer.WriteStringValue(value.ToString(DefaultFormat, null));
        }
    }
}
