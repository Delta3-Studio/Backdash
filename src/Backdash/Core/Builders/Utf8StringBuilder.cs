using System.Runtime.CompilerServices;

namespace Backdash.Core;

readonly ref struct Utf8StringBuilder
{
    readonly Span<byte> buffer;
    readonly ref int offset;

    public Utf8StringBuilder(in Span<byte> bufferArg, ref int offset)
    {
        buffer = bufferArg;
        this.offset = ref offset;
    }

    Span<byte> CurrentBuffer => offset >= buffer.Length ? [] : buffer[offset..];

    public bool Write(ReadOnlySpan<char> value)
    {
        var dest = CurrentBuffer;
        if (dest.IsEmpty) return false;
        var size = System.Text.Encoding.UTF8.GetByteCount(value);
        var chars = size <= dest.Length
            ? value
            : value[..dest.Length];
        offset += System.Text.Encoding.UTF8.GetBytes(chars, dest);
        return true;
    }

    public bool Write(ReadOnlySpan<byte> value)
    {
        var dest = CurrentBuffer;
        if (dest.IsEmpty) return false;
        var bytes = value.Length <= dest.Length
            ? value
            : value[..dest.Length];
        bytes.CopyTo(dest);
        offset += value.Length;
        return true;
    }

    public bool Write<T>(in T value, ReadOnlySpan<char> format, IFormatProvider? provider = null)
        where T : IUtf8SpanFormattable
    {
        var dest = CurrentBuffer;
        if (dest.IsEmpty) return false;
        if (!value.TryFormat(dest, out var written, format, provider))
            return false;
        offset += written;
        return true;
    }

    public bool Write<T>(in T value) where T : IUtf8SpanFormattable => Write(in value, []);

    const int MaxLocalStringSize = 24;

    public bool WriteFormat<T>(in T value, ReadOnlySpan<char> format = default) where T : ISpanFormattable
    {
        var dest = CurrentBuffer;
        if (dest.IsEmpty) return false;
        Span<char> charBuffer = stackalloc char[MaxLocalStringSize];
        return value.TryFormat(charBuffer, out var written, format, null) && Write(charBuffer[..written]);
    }

    public bool WriteEnum<T>(in T value, ReadOnlySpan<char> format = default) where T : struct, Enum
    {
        var dest = CurrentBuffer;
        if (dest.IsEmpty) return false;
        Span<char> charBuffer = stackalloc char[MaxLocalStringSize];
        return Enum.TryFormat(value, charBuffer, out var written, format) && Write(charBuffer[..written]);
    }
}

readonly ref struct Utf8ObjectStringWriter
{
    readonly Utf8StringBuilder writer;
    readonly int firstOffset;
    readonly ref int offset;

    public Utf8ObjectStringWriter(in Span<byte> bufferArg, ref int offset)
    {
        writer = new(in bufferArg, ref offset);
        this.offset = ref offset;
        writer.Write("{"u8);
        firstOffset = offset;
    }

    public bool Write<T>(
        in T value,
        ReadOnlySpan<char> format = default,
        [CallerArgumentExpression(nameof(value))]
        string name = ""
    ) where T : IUtf8SpanFormattable
    {
        if (firstOffset != offset && !writer.Write(", "u8)) return false;
        if (!writer.Write(name)) return false;
        if (!writer.Write(": "u8)) return false;
        return writer.Write(value, format);
    }

    public void Dispose() => writer.Write("}"u8);
}
