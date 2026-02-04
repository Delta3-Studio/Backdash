using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backdash.Core;

namespace Backdash;

/// <summary>
///     Value representation of a frame range (inclusive)
/// </summary>
[Serializable]
[JsonConverter(typeof(FrameRangeJsonConverter))]
public readonly record struct FrameRange(Frame Start, Frame End)
    : IComparable<FrameRange>,
        IComparable,
        IUtf8SpanFormattable,
        ISpanFormattable,
        IComparisonOperators<FrameRange, FrameRange, bool>,
        IAdditionOperators<FrameRange, FrameSpan, FrameRange>,
        ISubtractionOperators<FrameRange, FrameSpan, FrameRange>
{
    /// <inheritdoc />
    public FrameRange(int start, int end) : this((Frame)start, (Frame)end) { }

    /// <inheritdoc />
    public FrameRange(Range range) : this(range.Start.Value, range.End.Value) { }

    /// <inheritdoc />
    public FrameRange(Frame start, FrameSpan duration) : this(start, start + (duration.Frames - 1)) { }

    /// <summary>
    /// Get or set the duration of the range
    /// </summary>
    public FrameSpan Duration => new(End.Number - Start.Number + 1);

    /// <summary>
    /// Checks if an int frame is contained in the range
    /// </summary>
    public bool Contains(int frame) => frame >= Start.Number && frame <= End.Number;

    /// <summary>
    /// Checks if a frame is contained in the range
    /// </summary>
    public bool Contains(Frame frame) => Contains(frame.Number);

    /// <summary>
    /// Returns a new range with the duration changed
    /// </summary>
    public FrameRange WithDuration(int duration) => new(Start, Start + (duration - 1));

    ///  <inheritdoc cref="WithDuration(int)"/>
    public FrameRange WithDuration(FrameSpan duration) => WithDuration(duration.Frames);

    sealed class FrameRangeJsonConverter : JsonConverter<FrameRange>
    {
        public override FrameRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.StartArray)
                throw new JsonException("Start of array expected");

            reader.Read();
            var start = reader.GetInt32();

            reader.Read();
            var end = reader.GetInt32();

            reader.Read();
            if (reader.TokenType is not JsonTokenType.EndArray)
                throw new JsonException("End of array expected");

            return new((Frame)start, (Frame)end);
        }

        public override void Write(Utf8JsonWriter writer, FrameRange value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.Start.Number);
            writer.WriteNumberValue(value.End.Number);
            writer.WriteEndArray();
        }
    }

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        format ??= "D";
        DefaultInterpolatedStringHandler handler = new(3, 2, formatProvider ?? CultureInfo.InvariantCulture);
        handler.AppendLiteral("[");
        handler.AppendFormatted(Start, format);
        handler.AppendLiteral("..");
        handler.AppendFormatted(End, format);
        handler.AppendLiteral("]");
        return handler.ToStringAndClear();
    }

    /// <inheritdoc />
    public override string ToString() => ToString(null, null);

    /// <inheritdoc />
    public bool TryFormat(
        Span<char> destination, out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        charsWritten = 0;
        SpanStringBuilder writer = new(destination, ref charsWritten, provider ?? CultureInfo.InvariantCulture);
        if (format.IsEmpty) format = "D";
        return writer.Write("[")
               && writer.Write(Start.Number, format)
               && writer.Write("..")
               && writer.Write(End.Number, format)
               && writer.Write("]");
    }

    /// <inheritdoc />
    public bool TryFormat(
        Span<byte> utf8Destination, out int bytesWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        bytesWritten = 0;
        if (format.IsEmpty) format = "D";
        Utf8StringBuilder writer = new(utf8Destination, ref bytesWritten);
        return writer.Write("[")
               && writer.Write(Start.Number, format)
               && writer.Write("..")
               && writer.Write(End.Number, format)
               && writer.Write("]");
    }

    static int Compare(FrameRange left, FrameRange right) => (left.Start, left.End).CompareTo((right.Start, right.End));

    /// <inheritdoc />
    public int CompareTo(FrameRange other) => Compare(this, other);

    /// <inheritdoc />
    int IComparable.CompareTo(object? obj) => obj is FrameRange other ? Compare(this, other) : -1;

    /// <inheritdoc />
    public static bool operator >(FrameRange left, FrameRange right) => Compare(left, right) > 0;

    /// <inheritdoc />
    public static bool operator >=(FrameRange left, FrameRange right) => Compare(left, right) >= 0;

    /// <inheritdoc />
    public static bool operator <(FrameRange left, FrameRange right) => Compare(left, right) < 0;

    /// <inheritdoc />
    public static bool operator <=(FrameRange left, FrameRange right) => Compare(left, right) <= 0;

    /// <inheritdoc />
    public static FrameRange operator +(FrameRange left, FrameSpan right) => new(left.Start, left.Duration + right);

    /// <inheritdoc />
    public static FrameRange operator -(FrameRange left, FrameSpan right) => new(left.Start, left.Duration - right);

    /// <summary>
    /// Converts a range to a frame range.
    /// </summary>
    public static implicit operator FrameRange(Range range) => new(range);

    /// <summary>
    /// Converts a frame tuple to a frame range.
    /// </summary>
    public static implicit operator FrameRange((Frame Start, Frame End) range) => new(range.Start, range.End);

    /// <summary>
    /// Converts a tuple of frame and frame-span to a frame range.
    /// </summary>
    public static implicit operator FrameRange((Frame Start, FrameSpan Duration) range) =>
        new(range.Start, range.Duration);
}
