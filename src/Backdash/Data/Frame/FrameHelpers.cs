using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Backdash.Core;

namespace Backdash;

static class FrameHelpers
{
    const string DefaultFormat = "D";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetString<T>(
        T first, T last,
        string start, string div, string end,
        string? format, IFormatProvider? formatProvider
    ) where T : struct, IFormattable
    {
        format ??= DefaultFormat;
        DefaultInterpolatedStringHandler handler = new(3, 2, formatProvider ?? CultureInfo.InvariantCulture);
        handler.AppendLiteral(start);
        handler.AppendFormatted(first, format);
        handler.AppendLiteral(div);
        handler.AppendFormatted(last, format);
        handler.AppendLiteral(end);
        return handler.ToStringAndClear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFormat<T>(
        T first, T last,
        string start, string div, string end,
        Span<char> destination, out int charsWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    ) where T : struct, ISpanFormattable
    {
        charsWritten = 0;
        SpanStringBuilder writer = new(destination, ref charsWritten, provider ?? CultureInfo.InvariantCulture);
        if (format.IsEmpty) format = DefaultFormat;
        return writer.Write(start)
               && writer.Write(first, format)
               && writer.Write(div)
               && writer.Write(last, format)
               && writer.Write(end);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFormat<T>(
        T first, T last,
        string start, string div, string end,
        Span<byte> utf8Destination, out int bytesWritten,
        [StringSyntax(StringSyntaxAttribute.NumericFormat)]
        ReadOnlySpan<char> format,
        IFormatProvider? provider
    ) where T : IUtf8SpanFormattable
    {
        bytesWritten = 0;
        provider ??= CultureInfo.InvariantCulture;
        if (format.IsEmpty) format = DefaultFormat;
        Utf8StringBuilder writer = new(utf8Destination, ref bytesWritten);
        return writer.Write(start)
               && writer.Write(first, format, provider)
               && writer.Write(div)
               && writer.Write(last, format, provider)
               && writer.Write(end);
    }
}
