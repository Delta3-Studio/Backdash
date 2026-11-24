// ReSharper disable InvocationIsCultureSensitive

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backdash.Serialization;

namespace Backdash;

/// <summary>
///     Value representation of a frame range (inclusive)
/// </summary>
[Serializable]
[DebuggerDisplay("{ToString()}")]
[JsonConverter(typeof(FrameCounterJsonConverter))]
public record struct FrameCounter : IUtf8SpanFormattable, ISpanFormattable, IBinarySerializable
{
    FrameSpan elapsedFrames;
    FrameSpan totalFrames;

    /// <summary>Current elapsed frames</summary>
    public readonly FrameSpan Elapsed => elapsedFrames;

    /// <summary>Total frames</summary>
    public readonly FrameSpan Total => totalFrames;

    /// <summary>
    ///     Value representation of a frame range (inclusive)
    /// </summary>
    public FrameCounter(FrameSpan elapsed, FrameSpan total)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(total.Frames);
        totalFrames = total;
        elapsedFrames = FrameSpan.Nat(elapsed, totalFrames);
    }

    /// <inheritdoc />
    public FrameCounter(Frame current, FrameSpan duration) : this((FrameSpan)current, duration) { }

    /// <inheritdoc />
    public FrameCounter(FrameSpan duration) : this(FrameSpan.Zero, duration) { }

    /// <inheritdoc />
    public FrameCounter(int current, int total) : this((FrameSpan)current, (FrameSpan)total) { }

    /// <inheritdoc />
    public FrameCounter(int total) : this((FrameSpan)total) { }

    /// <inheritdoc />
    public FrameCounter(TimeSpan current, TimeSpan total) : this(FrameSpan.FromDuration(current), FrameSpan.FromDuration(total)) { }

    /// <inheritdoc />
    public FrameCounter(TimeSpan total) : this(FrameSpan.FromDuration(total)) { }

    /// <summary>Get the current frame</summary>
    public readonly Frame Current => elapsedFrames.ToFrame();

    /// <summary>Get remaining frames to total</summary>
    public readonly FrameSpan Remaining => totalFrames - elapsedFrames;

    /// <summary>Check if counter is completed</summary>
    public readonly bool IsCompleted() => elapsedFrames.Frames >= totalFrames.Frames;

    /// <summary>Check if the counter not started</summary>
    public readonly bool IsFirstFrame() => elapsedFrames.IsZero;

    /// <summary>Reset the elapsed counter</summary>
    public void Reset() => elapsedFrames = FrameSpan.Zero;

    /// <summary>Set the elapsed equal to total</summary>
    public void Complete() => elapsedFrames = totalFrames;

    /// <summary>Set the total elapsed frames</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetElapsed(FrameSpan amount) => elapsedFrames = FrameSpan.Nat(amount, totalFrames);

    /// <inheritdoc cref="SetElapsed(FrameSpan)"/>
    public void SetElapsed(Frame frame) => SetElapsed(frame.ToSpan());

    /// <inheritdoc cref="SetElapsed(FrameSpan)"/>
    public void SetElapsed(int frames) => SetElapsed((FrameSpan)frames);

    /// <inheritdoc cref="SetElapsed(FrameSpan)"/>
    public void SetElapsed(TimeSpan duration) => SetElapsed(FrameSpan.FromDuration(duration));

    /// <summary>Set the total frames</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTotal(FrameSpan amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount.Frames);
        totalFrames = FrameSpan.Nat(amount, totalFrames);
        elapsedFrames = FrameSpan.Min(elapsedFrames, totalFrames);
    }

    /// <inheritdoc cref="SetTotal(FrameSpan)"/>
    public void SetTotal(Frame frame) => SetTotal(frame.ToSpan());

    /// <inheritdoc cref="SetTotal(FrameSpan)"/>
    public void SetTotal(int frames) => SetTotal((FrameSpan)frames);

    /// <inheritdoc cref="SetTotal(FrameSpan)"/>
    public void SetTotal(TimeSpan duration) => SetTotal(FrameSpan.FromDuration(duration));

    /// <summary>Advance elapsed counter by 1 frame</summary>
    public void Advance() => SetElapsed(elapsedFrames + FrameSpan.One);

    /// <summary>Advance elapsed counter by <paramref name="amount"/></summary>
    public void Advance(FrameSpan amount) => SetElapsed(elapsedFrames + amount);

    /// <inheritdoc cref="Advance(FrameSpan)"/>
    public void Advance(int amount) => SetElapsed(elapsedFrames + amount);

    /// <inheritdoc cref="Advance(FrameSpan)"/>
    public void Advance(Frame frame) => SetElapsed(elapsedFrames + frame);

    /// <inheritdoc cref="Advance(FrameSpan)"/>
    public void Advance(TimeSpan duration) => SetElapsed(elapsedFrames + FrameSpan.FromDuration(duration));

    /// <summary>Go back elapsed counter by 1 frame</summary>
    public void Recede() => SetElapsed(elapsedFrames - FrameSpan.One);

    /// <summary>Go back elapsed counter by <paramref name="amount"/></summary>
    public void Recede(FrameSpan amount) => SetElapsed(elapsedFrames - amount);

    /// <inheritdoc cref="Recede(Backdash.FrameSpan)"/>
    public void Recede(int amount) => SetElapsed(elapsedFrames - amount);

    /// <inheritdoc cref="Recede(Backdash.FrameSpan)"/>
    public void Recede(Frame frame) => SetElapsed(elapsedFrames - frame);

    /// <inheritdoc cref="Recede(FrameSpan)"/>
    public void Recede(TimeSpan duration) => SetElapsed(elapsedFrames - FrameSpan.FromDuration(duration));

    /// <summary>Deconstructor</summary>
    public readonly void Deconstruct(out FrameSpan elapsed, out FrameSpan total)
    {
        elapsed = elapsedFrames;
        total = totalFrames;
    }

    /// <inheritdoc />
    public readonly void Serialize(ref readonly BinaryBufferWriter writer)
    {
        writer.Write(in elapsedFrames);
        writer.Write(in totalFrames);
    }

    /// <inheritdoc />
    public void Deserialize(ref readonly BinaryBufferReader reader)
    {
        reader.Read(ref elapsedFrames);
        reader.Read(ref totalFrames);
    }

    /// <inheritdoc />
    public override readonly string ToString() => ToString(null, null);

    /// <inheritdoc />
    public readonly string ToString(string? format, IFormatProvider? formatProvider) =>
        FrameHelpers.GetString(
            elapsedFrames, totalFrames, "(", "/", ")",
            format, formatProvider
        );

    /// <inheritdoc />
    public readonly bool TryFormat(
        Span<char> destination, out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format,
        IFormatProvider? provider) =>
        FrameHelpers.TryFormat(
            elapsedFrames, totalFrames, "(", "/", ")",
            destination, out charsWritten, format, provider
        );

    /// <inheritdoc />
    public readonly bool TryFormat(
        Span<byte> utf8Destination, out int bytesWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format,
        IFormatProvider? provider) =>
        FrameHelpers.TryFormat(
            elapsedFrames, totalFrames, "(", "/", ")",
            utf8Destination, out bytesWritten, format, provider
        );

    sealed class FrameCounterJsonConverter : JsonConverter<FrameCounter>
    {
        public override FrameCounter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType is not JsonTokenType.StartArray) throw new JsonException("Start of array expected");
            reader.Read();
            var elapsed = reader.GetFrameSpan();
            reader.Read();
            var total = reader.GetFrameSpan();
            reader.Read();
            if (reader.TokenType is not JsonTokenType.EndArray) throw new JsonException("End of array expected");
            return new(elapsed, total);
        }

        public override void Write(Utf8JsonWriter writer, FrameCounter value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteFrameSpan(value.elapsedFrames);
            writer.WriteFrameSpan(value.totalFrames);
            writer.WriteEndArray();
        }
    }
}
