#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Backdash.Core;

/// <summary>
/// Write values into a span buffer
/// </summary>
public readonly ref struct SpanStringBuilder(
    in Span<char> destination,
    ref int writtenCount,
    IFormatProvider? formatProvider = null
)
{
    readonly Span<char> buffer = destination;
    readonly ref int offset = ref writtenCount;

    Span<char> CurrentBuffer => offset >= buffer.Length ? [] : buffer[offset..];
    Span<char> WrittenSpan => buffer[..offset];

    public override string ToString() => new(WrittenSpan);

    public void Reset() => offset = 0;

    public void Clear()
    {
        WrittenSpan.Clear();
        Reset();
    }

    public bool Write(scoped ReadOnlySpan<char> value)
    {
        if (!value.TryCopyTo(CurrentBuffer))
            return false;

        offset += value.Length;
        return true;
    }

    public bool Write<T>(in T value, ReadOnlySpan<char> format) where T : ISpanFormattable
    {
        if (!value.TryFormat(CurrentBuffer, out int written, format, formatProvider))
            return false;

        offset += written;
        return true;
    }

    public bool Write<T>(in T value) where T : ISpanFormattable => Write(in value, []);

    public bool WriteEnum<T>(in T value, ReadOnlySpan<char> format = default) where T : struct, Enum
    {
        if (!Enum.TryFormat(value, CurrentBuffer, out var written, format))
            return false;

        offset += written;
        return true;
    }

#pragma warning disable S2325, CA1822
    public bool Append([InterpolatedStringHandlerArgument("")] ref InterpolatedStringHandler handler) =>
        handler.Succeeded;
#pragma warning restore S2325, CA1822

    [InterpolatedStringHandler]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public ref struct InterpolatedStringHandler
    {
        readonly SpanStringBuilder builder;
        bool succeeded = true;

        public readonly bool Succeeded => succeeded;

        public InterpolatedStringHandler(int literalLength, int formattedCount, SpanStringBuilder builder) =>
            this.builder = builder;

        public void AppendLiteral(string value)
        {
            if (succeeded)
                succeeded = succeeded && builder.Write(value);
        }

        public void AppendLiteral(ReadOnlySpan<char> value)
        {
            if (succeeded)
                succeeded = succeeded && builder.Write(value);
        }

        public void AppendFormatted<T>(T value) where T : ISpanFormattable
        {
            if (succeeded)
                succeeded = succeeded && builder.Write(value);
        }

        public void AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
        {
            if (succeeded)
                succeeded = succeeded && builder.Write(value, format);
        }
    }
}
