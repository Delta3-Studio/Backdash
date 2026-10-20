using System.Numerics;
using System.Runtime.CompilerServices;
using Backdash.Core;
using Backdash.Core.Json;

namespace Backdash.Data;

/// <summary>
///     Represents a byte size value
/// </summary>
[UnsafeInt64JsonConverter<ByteSize>]
public readonly record struct ByteSize(long TotalBytes)
    :
        IComparable<ByteSize>,
        ISpanFormattable,
        IUtf8SpanFormattable,
        IComparisonOperators<ByteSize, ByteSize, bool>,
        IAdditionOperators<ByteSize, ByteSize, ByteSize>,
        ISubtractionOperators<ByteSize, ByteSize, ByteSize>,
        IDivisionOperators<ByteSize, long, ByteSize>,
        IDivisionOperators<ByteSize, double, ByteSize>,
        IMultiplyOperators<ByteSize, long, ByteSize>,
        IIncrementOperators<ByteSize>,
        IDecrementOperators<ByteSize>
{
    /// <summary>Gets the byte value <c>1</c>.</summary>
    public static ByteSize One { get; } = new(1);

    /// <summary>Gets the byte value <c>0</c>.</summary>
    public static ByteSize Zero { get; } = new(0);

    /// <summary>Gets the number of bits in one byte.</summary>
    public const ushort ByteToBits = 8;

    /// <summary>Gets the number of bytes in one kibibyte.</summary>
    public const double BytesToKibiByte = 1_024;

    /// <summary>Gets the number of bytes in one mebibyte.</summary>
    public const double BytesToMebiByte = 1_048_576;

    /// <summary>Gets the number of bytes in one gibibyte.</summary>
    public const double BytesToGibiByte = 1_073_741_824;

    /// <summary>Gets the number of bytes in one kilobyte.</summary>
    public const double BytesToKiloByte = 1_000;

    /// <summary>Gets the number of bytes in one megabyte.</summary>
    public const double BytesToMegaByte = 1_000_000;

    /// <summary>Gets the number of bytes in one gigabyte.</summary>
    public const double BytesToGigaByte = 1_000_000_000;

    /// <summary>Gets the number of bytes in one terabyte.</summary>
    public const double BytesToTeraByte = 1_000_000_000_000;

    /// <summary>Gets the number of bytes in one tebibyte.</summary>
    public const double BytesToTebiByte = 1_099_511_627_776;

    /// <summary>Gets the symbol for bytes.</summary>
    public const string ByteSymbol = "B";

    /// <summary>Gets the symbol for kibibytes.</summary>
    public const string KibiByteSymbol = "KiB";

    /// <summary>Gets the symbol for mebibytes.</summary>
    public const string MebiByteSymbol = "MiB";

    /// <summary>Gets the symbol for gibibytes.</summary>
    public const string GibiByteSymbol = "GiB";

    /// <summary>Gets the symbol for tebibytes.</summary>
    public const string TebiByteSymbol = "TiB";

    /// <summary>Gets the symbol for kilobytes.</summary>
    public const string KiloByteSymbol = "KB";

    /// <summary>Gets the symbol for megabytes.</summary>
    public const string MegaByteSymbol = "MB";

    /// <summary>Gets the symbol for gigabytes.</summary>
    public const string GigaByteSymbol = "GB";

    /// <summary>Gets the symbol for terabytes.</summary>
    public const string TeraByteSymbol = "TB";

    /// <summary>Gets the <see cref="int"/> clamped number of bytes.</summary>
    public int Bytes
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => int.CreateSaturating(TotalBytes);
    }

    /// <summary>Gets the number of KibiBytes represented by this object.</summary>
    public double KibiBytes => TotalBytes / BytesToKibiByte;

    /// <summary>Gets the number of MebiBytes represented by this object.</summary>
    public double MebiBytes => TotalBytes / BytesToMebiByte;

    /// <summary>Gets the number of GibiBytes represented by this object.</summary>
    public double GibiBytes => TotalBytes / BytesToGibiByte;

    /// <summary>Gets the number of TebiBytes represented by this object.</summary>
    public double TebiBytes => TotalBytes / BytesToTebiByte;

    /// <summary>Gets the number of KiloBytes represented by this object.</summary>
    public double KiloBytes => TotalBytes / BytesToKiloByte;

    /// <summary>Gets the number of MegaBytes represented by this object.</summary>
    public double MegaBytes => TotalBytes / BytesToMegaByte;

    /// <summary>Gets the number of GigaBytes represented by this object.</summary>
    public double GigaBytes => TotalBytes / BytesToGigaByte;

    /// <summary>Gets the number of TeraBytes represented by this object.</summary>
    public double TeraBytes => TotalBytes / BytesToTeraByte;

    /// <inheritdoc />
    public int CompareTo(ByteSize other) => TotalBytes.CompareTo(other.TotalBytes);

    ReadOnlySpan<char> GetMaxBinarySymbol()
    {
        if (Math.Abs(TebiBytes) >= 1) return TebiByteSymbol;
        if (Math.Abs(GibiBytes) >= 1) return GibiByteSymbol;
        if (Math.Abs(MebiBytes) >= 1) return MebiByteSymbol;
        if (Math.Abs(KibiBytes) >= 1) return KibiByteSymbol;
        return ByteSymbol;
    }

    ReadOnlySpan<char> GetMaxDecimalSymbol()
    {
        if (Math.Abs(TeraBytes) >= 1) return TeraByteSymbol;
        if (Math.Abs(GigaBytes) >= 1) return GigaByteSymbol;
        if (Math.Abs(MegaBytes) >= 1) return MegaByteSymbol;
        if (Math.Abs(KiloBytes) >= 1) return KiloByteSymbol;
        return ByteSymbol;
    }

    double GetValue(Measure measure) =>
        measure switch
        {
            Measure.Byte => TotalBytes,
            Measure.KibiByte => KibiBytes,
            Measure.MebiByte => MebiBytes,
            Measure.GibiByte => GibiBytes,
            Measure.TebiByte => TebiBytes,
            Measure.KiloByte => KiloBytes,
            Measure.MegaByte => MegaBytes,
            Measure.GigaByte => GigaBytes,
            Measure.TeraByte => TeraBytes,
            _ => TotalBytes,
        };

    double GetValueForSymbol(ReadOnlySpan<char> symbol) => GetValue(SymbolToMeasure(symbol));
    const string DefaultFormat = "0.##";
    const string BinaryFormat = "binary";
    const string DecimalFormat = "decimal";

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        format ??= DecimalFormat;
        if (format.Equals(BinaryFormat, StringComparison.OrdinalIgnoreCase))
        {
            var maxBinarySymbol = GetMaxBinarySymbol();
            var binaryValue = GetValueForSymbol(maxBinarySymbol);
            return binaryValue.ToString($"{DefaultFormat} {maxBinarySymbol}", formatProvider);
        }

        if (format.Equals(DecimalFormat, StringComparison.OrdinalIgnoreCase))
        {
            var maxDecimalSymbol = GetMaxDecimalSymbol();
            var decimalValue = GetValueForSymbol(maxDecimalSymbol);
            return decimalValue.ToString($"{DefaultFormat} {maxDecimalSymbol}", formatProvider);
        }

        var symbol = FindSymbol(format);
        if (symbol.IsEmpty)
            return TotalBytes.ToString(format, formatProvider);
        var value = GetValueForSymbol(symbol);
        if (
            (!format.Contains('#') && !format.Contains('0'))
            || symbol.Equals(format, StringComparison.OrdinalIgnoreCase)
        )
            return value.ToString($"{DefaultFormat} {symbol}", formatProvider);
        string symbolString = new(symbol);
        return value.ToString(
            format.Replace(symbolString, symbolString, StringComparison.OrdinalIgnoreCase),
            formatProvider
        );
    }

    /// <summary>
    ///     Returns the string representation for the current byte size
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <inheritdoc cref="ToString(string?,System.IFormatProvider?)" />
    public string ToString(string? format) => ToString(format, null);

    /// <summary>
    ///     Returns the string representation for the current byte size as <paramref name="measure" />
    /// </summary>
    /// <para name="measure">The unit of measure conversion</para>
    public string ToString(Measure measure) => ToString(MeasureToSymbol(measure));

    /// <inheritdoc cref="ToString(string?,System.IFormatProvider?)" />
    public bool TryFormat(
        Span<byte> utf8Destination,
        out int bytesWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        bytesWritten = 0;
        Utf8StringBuilder writer = new(in utf8Destination, ref bytesWritten);
        format = format.IsEmpty ? DecimalFormat : format;
        if (format.Equals(BinaryFormat, StringComparison.OrdinalIgnoreCase))
        {
            var maxBinarySymbol = GetMaxBinarySymbol();
            var binaryValue = GetValueForSymbol(maxBinarySymbol);
            return writer.Write(binaryValue, DefaultFormat, provider) &&
                   writer.Write(" "u8) &&
                   writer.Write(maxBinarySymbol);
        }

        if (format.Equals(DecimalFormat, StringComparison.OrdinalIgnoreCase))
        {
            var maxDecimalSymbol = GetMaxDecimalSymbol();
            var decimalValue = GetValueForSymbol(maxDecimalSymbol);
            return writer.Write(decimalValue, DefaultFormat, provider) &&
                   writer.Write(" "u8) &&
                   writer.Write(maxDecimalSymbol);
        }

        var symbol = FindSymbol(format);
        if (symbol.IsEmpty)
            return writer.Write(TotalBytes, format, provider);

        var value = GetValueForSymbol(symbol);
        if ((!format.Contains('#') && !format.Contains('0'))
            || symbol.Equals(format, StringComparison.OrdinalIgnoreCase))
            return writer.Write(value, DefaultFormat, provider) &&
                   writer.Write(" "u8) &&
                   writer.Write(symbol);

        return writer.Write(value, format, provider);
    }

    /// <inheritdoc />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        charsWritten = 0;
        SpanStringBuilder writer = new(in destination, ref charsWritten, provider);
        format = format.IsEmpty ? DecimalFormat : format;
        if (format.Equals(BinaryFormat, StringComparison.OrdinalIgnoreCase))
        {
            var maxBinarySymbol = GetMaxBinarySymbol();
            var binaryValue = GetValueForSymbol(maxBinarySymbol);
            return writer.Write(binaryValue, DefaultFormat) &&
                   writer.Write(" ") &&
                   writer.Write(maxBinarySymbol);
        }

        if (format.Equals(DecimalFormat, StringComparison.OrdinalIgnoreCase))
        {
            var maxDecimalSymbol = GetMaxDecimalSymbol();
            var decimalValue = GetValueForSymbol(maxDecimalSymbol);
            return writer.Write(decimalValue, DefaultFormat) &&
                   writer.Write(" ") &&
                   writer.Write(maxDecimalSymbol);
        }

        var symbol = FindSymbol(format);
        if (symbol.IsEmpty)
            return writer.Write(TotalBytes, format);

        var value = GetValueForSymbol(symbol);
        if ((!format.Contains('#') && !format.Contains('0'))
            || symbol.Equals(format, StringComparison.OrdinalIgnoreCase))
            return writer.Write(value, DefaultFormat) &&
                   writer.Write(" ") &&
                   writer.Write(symbol);

        return writer.Write(value, format);
    }

    /// <summary>Cast <see cref="ByteSize"/> to <see cref="long"/> byte-count value.</summary>
    public static explicit operator long(ByteSize size) => size.TotalBytes;

    /// <summary>Cast <see cref="ByteSize"/> to <see cref="int"/> byte-count value.</summary>
    public static explicit operator int(ByteSize size) => size.Bytes;

    /// <inheritdoc />
    public static bool operator >(ByteSize left, ByteSize right) => left.TotalBytes > right.TotalBytes;

    /// <inheritdoc />
    public static bool operator >=(ByteSize left, ByteSize right) => left.TotalBytes >= right.TotalBytes;

    /// <inheritdoc />
    public static bool operator <(ByteSize left, ByteSize right) => left.TotalBytes < right.TotalBytes;

    /// <inheritdoc />
    public static bool operator <=(ByteSize left, ByteSize right) => left.TotalBytes <= right.TotalBytes;

    /// <inheritdoc />
    public static ByteSize operator ++(ByteSize value) => new(value.TotalBytes + 1);

    /// <inheritdoc />
    public static ByteSize operator --(ByteSize value) => new(value.TotalBytes - 1);

    /// <inheritdoc />
    public static ByteSize operator +(ByteSize left, ByteSize right) => new(left.TotalBytes + right.TotalBytes);

    /// <inheritdoc />
    public static ByteSize operator -(ByteSize left, ByteSize right) => new(left.TotalBytes - right.TotalBytes);

    /// <inheritdoc />
    public static ByteSize operator /(ByteSize left, double right) => new((long)(left.TotalBytes / right));

    /// <inheritdoc />
    public static ByteSize operator /(ByteSize left, long right) => left / (double)right;

    /// <inheritdoc />
    public static ByteSize operator *(ByteSize left, long right) => new(left.TotalBytes * right);

    /// <inheritdoc cref="op_Multiply(ByteSize,long)" />
    public static ByteSize operator *(long left, ByteSize right) => new(left * right.TotalBytes);

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> bytes
    /// </summary>
    /// <param name="value"></param>
    public static explicit operator ByteSize(long value) => new(value);

    /// <inheritdoc cref="op_Explicit(long)" />
    public static explicit operator ByteSize(int value) => new(value);

    /// <inheritdoc cref="op_Explicit(long)" />
    public static explicit operator ByteSize(uint value) => new(value);

    /// <inheritdoc cref="op_Explicit(long)" />
    public static explicit operator ByteSize(short value) => new(value);

    /// <inheritdoc cref="op_Explicit(long)" />
    public static explicit operator ByteSize(ushort value) => new(value);

    /// <inheritdoc cref="op_Explicit(long)" />
    public static explicit operator ByteSize(sbyte value) => new(value);

    /// <inheritdoc cref="op_Explicit(long)" />
    public static explicit operator ByteSize(byte value) => new(value);

    static ReadOnlySpan<char> FindSymbol(ReadOnlySpan<char> str)
    {
        const StringComparison cmp = StringComparison.Ordinal;
        if (str.Contains(KibiByteSymbol, cmp)) return KibiByteSymbol;
        if (str.Contains(MebiByteSymbol, cmp)) return MebiByteSymbol;
        if (str.Contains(GibiByteSymbol, cmp)) return GibiByteSymbol;
        if (str.Contains(TebiByteSymbol, cmp)) return TebiByteSymbol;
        if (str.Contains(KiloByteSymbol, cmp)) return KiloByteSymbol;
        if (str.Contains(MegaByteSymbol, cmp)) return MegaByteSymbol;
        if (str.Contains(GigaByteSymbol, cmp)) return GigaByteSymbol;
        if (str.Contains(TeraByteSymbol, cmp)) return TeraByteSymbol;
        if (str.Contains(ByteSymbol, cmp)) return ByteSymbol;
        return [];
    }

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> bytes
    /// </summary>
    /// <param name="value">Number of bytes</param>
    public static ByteSize FromBytes(long value) => new(value);

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> kilo-bytes
    /// </summary>
    /// <param name="value">Number of kilobytes</param>
    public static ByteSize FromKiloByte(double value) => new((long)(value * BytesToKiloByte));

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> mega-bytes
    /// </summary>
    /// <param name="value">Number of megabytes</param>
    public static ByteSize FromMegaBytes(double value) => new((long)(value * BytesToMegaByte));

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> giga-bytes
    /// </summary>
    /// <param name="value">Number of gigabytes</param>
    public static ByteSize FromGigaBytes(double value) => new((long)(value * BytesToGigaByte));

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> tera-bytes
    /// </summary>
    /// <param name="value">Number of terabytes</param>
    public static ByteSize FromTeraBytes(double value) => new((long)(value * BytesToTeraByte));

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> kibi-bytes
    /// </summary>
    /// <param name="value">Number of kibibytes</param>
    public static ByteSize FromKibiBytes(double value) => new((long)(value * BytesToKibiByte));

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> mebi-bytes
    /// </summary>
    /// <param name="value">Number of mebibytes</param>
    public static ByteSize FromMebiBytes(double value) => new((long)(value * BytesToMebiByte));

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> gibi-bytes
    /// </summary>
    /// <param name="value">Number of gibibytes</param>
    public static ByteSize FromGibiBytes(double value) => new((long)(value * BytesToGibiByte));

    /// <summary>
    ///     Returns new <see cref="ByteSize" /> with <paramref name="value" /> gibi-bytes
    /// </summary>
    /// <param name="value">Number of tebibytes</param>
    public static ByteSize FromTebiBytes(double value) => new((long)(value * BytesToTebiByte));

    /// <summary>
    ///     Returns number of bits for <paramref name="byteCount" /> bytes.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BytesToBits(ushort byteCount) => byteCount * ByteToBits;

    /// <summary>
    ///     Returns the number of bytes required to store <paramref name="bitCount" /> bits.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BitsToBytes(ushort bitCount) => MathI.CeilDiv(bitCount, ByteToBits);

    /// <summary>
    ///     Returns the size of a value of the given type parameter.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ByteSize OfType<T>() where T : unmanaged => new(Unsafe.SizeOf<T>());

    /// <summary>
    ///     Returns the first power of two ByteSize greater than <paramref name="value" />
    /// </summary>
    public static ByteSize NextPowerOfTwo(ByteSize value) => new(MathI.NextPowerOfTwo(value.TotalBytes));

    /// <summary>
    ///     Returns the first power of two ByteSize greater than <paramref name="size" /> (in bytes)
    /// </summary>
    public static ByteSize NextPowerOfTwo(int size) => size <= 0 ? Zero : new(MathI.NextPowerOfTwo(size));

    static Measure SymbolToMeasure(ReadOnlySpan<char> symbol) =>
        symbol switch
        {
            KibiByteSymbol => Measure.KibiByte,
            MebiByteSymbol => Measure.MebiByte,
            GibiByteSymbol => Measure.GibiByte,
            TebiByteSymbol => Measure.TebiByte,
            KiloByteSymbol => Measure.KiloByte,
            MegaByteSymbol => Measure.MegaByte,
            GigaByteSymbol => Measure.GigaByte,
            TeraByteSymbol => Measure.TeraByte,
            ByteSymbol => Measure.Byte,
            _ => Measure.Unknown,
        };

    static string MeasureToSymbol(Measure measure) =>
        measure switch
        {
            Measure.KibiByte => KibiByteSymbol,
            Measure.MebiByte => MebiByteSymbol,
            Measure.GibiByte => GibiByteSymbol,
            Measure.TebiByte => TebiByteSymbol,
            Measure.KiloByte => KiloByteSymbol,
            Measure.MegaByte => MegaByteSymbol,
            Measure.GigaByte => GigaByteSymbol,
            Measure.TeraByte => TeraByteSymbol,
            Measure.Byte => ByteSymbol,
            _ => string.Empty,
        };

    /// <summary>
    ///     Unit of measure for <see cref="ByteSize" />
    /// </summary>
    public enum Measure : sbyte
    {
        /// <summary>
        ///     Byte
        /// </summary>
        Byte = 0,

        /// <summary>
        ///     1KiB == 1024 bytes
        /// </summary>
        KibiByte,

        /// <summary>
        ///     1MiB == 1_048_576 bytes
        /// </summary>
        MebiByte,

        /// <summary>
        ///     1GiB == 1_073_741_824 bytes
        /// </summary>
        GibiByte,

        /// <summary>
        ///     1TiB == 1_099_511_627_776 bytes
        /// </summary>
        TebiByte,

        /// <summary>
        ///     1KB == 1000 bytes
        /// </summary>
        KiloByte,

        /// <summary>
        ///     1MB == 1_000_000 bytes
        /// </summary>
        MegaByte,

        /// <summary>
        ///     1GB == 1_000_000_000 bytes
        /// </summary>
        GigaByte,

        /// <summary>
        ///     1TB == 1_000_000_000_000 bytes
        /// </summary>
        TeraByte,

        /// <summary>
        ///     Unknown unit of measure
        /// </summary>
        Unknown = -1,
    }
}
