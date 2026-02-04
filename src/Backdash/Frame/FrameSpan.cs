using System.Diagnostics;
using System.Numerics;
using Backdash.Core;

namespace Backdash;

/// <summary>
///     Value representation of a frame count
///     Uses the FPS defined in <seealso cref="FrameTime"/>.<see cref="FrameTime.CurrentFrameRate"/>
/// </summary>
/// <seealso cref="FrameTime.set_CurrentFrameRate"/>
[Serializable]
[UnsafeInt32JsonConverter<FrameSpan>]
[DebuggerDisplay("{ToString()}")]
public readonly record struct FrameSpan :
    IComparable<FrameSpan>,
    IUtf8SpanFormattable,
    ISpanFormattable,
    IComparisonOperators<FrameSpan, FrameSpan, bool>,
    IAdditionOperators<FrameSpan, FrameSpan, FrameSpan>,
    ISubtractionOperators<FrameSpan, FrameSpan, FrameSpan>,
    IModulusOperators<FrameSpan, int, FrameSpan>,
    IAdditionOperators<FrameSpan, int, FrameSpan>,
    IMultiplyOperators<FrameSpan, int, FrameSpan>,
    ISubtractionOperators<FrameSpan, int, FrameSpan>,
    IAdditionOperators<FrameSpan, Frame, FrameSpan>,
    ISubtractionOperators<FrameSpan, Frame, FrameSpan>,
    IAdditionOperators<FrameSpan, FrameRange, FrameRange>,
    ISubtractionOperators<FrameSpan, FrameRange, FrameRange>
{
    /// <summary>Return frame span of <c>0</c> frames</summary>
    public static readonly FrameSpan Zero = new(0);

    /// <summary>Return frame span of <c>1</c> frame</summary>
    public static readonly FrameSpan One = new(1);

    /// <summary>Returns max frame span value</summary>
    public static readonly FrameSpan MaxValue = new(int.MaxValue);

    /// <summary>Returns the <see cref="int" /> count of frames in the current frame span <see cref="Frame" />.</summary>
    public readonly int Frames = 0;

    /// <summary>
    ///     Initialize new <see cref="FrameSpan" /> for frame <paramref name="frames" />.
    /// </summary>
    /// <param name="frames"></param>
    public FrameSpan(int frames) => Frames = frames;

    /// <summary>Returns the time value for the current frame span in seconds.</summary>
    public double Seconds(int fps) => FrameTime.GetSeconds(Frames, fps);

    /// <summary>Returns the time value for the current frame span in seconds.</summary>
    public double Seconds() => FrameTime.GetSeconds(Frames);

    /// <summary>Returns the time value for the current frame span in <see cref="TimeSpan" />.</summary>
    public TimeSpan Duration(int fps) => FrameTime.GetDuration(Frames, fps);

    /// <summary>Returns the time value for the current frame span in <see cref="TimeSpan" />.</summary>
    public TimeSpan Duration() => FrameTime.GetDuration(Frames);

    /// <summary>Returns the value for the current frame span as a <see cref="Frame" />.</summary>
    public Frame ToFrame() => new(Frames);

    /// <summary>
    ///     Returns a frame at the time position in milliseconds
    /// </summary>
    public Frame GetFrameAtMilliSecond(double millis, int fps)
    {
        var span = FromMilliseconds(millis, fps);
        if (span.Frames > Frames)
            throw new InvalidOperationException("Out of range frame time");

        return span.ToFrame();
    }

    /// <summary>
    ///     Returns a frame at the time position in milliseconds
    /// </summary>
    public Frame GetFrameAtMilliSecond(double millis)
    {
        var span = FromMilliseconds(millis);
        if (span.Frames > Frames)
            throw new InvalidOperationException("Out of range frame time");

        return span.ToFrame();
    }

    /// <summary>
    ///     Returns a frame at the time position in seconds
    /// </summary>
    public Frame GetFrameAtSecond(double seconds, int fps)
    {
        var span = FromSeconds(seconds, fps);
        if (span.Frames > Frames)
            throw new InvalidOperationException("Out of range frame time");

        return span.ToFrame();
    }

    /// <summary>
    ///     Returns a frame at the time position in seconds
    /// </summary>
    public Frame GetFrameAtSecond(double seconds)
    {
        var span = FromSeconds(seconds);
        if (span.Frames > Frames)
            throw new InvalidOperationException("Out of range frame time");

        return span.ToFrame();
    }

    /// <summary>
    ///     Returns a frame at the timespan position
    /// </summary>
    public Frame GetFrameAt(TimeSpan duration, int fps) => GetFrameAtMilliSecond(duration.TotalMilliseconds, fps);

    /// <summary>
    ///     Returns a frame at the timespan position
    /// </summary>
    public Frame GetFrameAt(TimeSpan duration) => GetFrameAtMilliSecond(duration.TotalMilliseconds);

    /// <inheritdoc />
    public int CompareTo(FrameSpan other) => Frames.CompareTo(other.Frames);

    const string DefaultFormat = "0 frames;-# frames";

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider) =>
        Frames.ToString(format ?? DefaultFormat, formatProvider);

    /// <inheritdoc />
    public override string ToString() => ToString(null, null);

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
        if (!writer.Write(Frames, format, provider)) return false;
        if (!writer.Write(" frames"u8)) return false;
        return true;
    }

    /// <inheritdoc />
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (format.IsEmpty) format = DefaultFormat;
        return Frames.TryFormat(destination, out charsWritten, format, provider);
    }

    /// <inheritdoc cref="FrameSpan(int)" />
    public static FrameSpan Of(int frameCount) => new(frameCount);

    /// <summary>
    ///     Returns new <see cref="FrameSpan" /> for <paramref name="seconds" /> at specified <paramref name="fps" />.
    /// </summary>
    public static FrameSpan FromSeconds(double seconds, int fps) =>
        new(FrameTime.GetFrames(seconds, fps));

    /// <summary>
    ///     Returns new <see cref="FrameSpan" /> for <paramref name="seconds" />
    /// </summary>
    public static FrameSpan FromSeconds(double seconds) => new(FrameTime.GetFrames(seconds));

    /// <summary>
    ///     Returns new <see cref="FrameSpan" /> for <paramref name="duration" /> at specified <paramref name="fps" />.
    /// </summary>
    public static FrameSpan FromTimeSpan(TimeSpan duration, int fps) =>
        new(FrameTime.GetFrames(duration, fps));

    /// <summary>
    ///     Returns new <see cref="FrameSpan" /> for <paramref name="duration" />.
    /// </summary>
    public static FrameSpan FromTimeSpan(TimeSpan duration) =>
        new(FrameTime.GetFrames(duration));

    /// <summary>
    ///     Returns new <see cref="FrameSpan" /> for <paramref name="milliseconds" /> at specified <paramref name="fps" />.
    /// </summary>
    public static FrameSpan FromMilliseconds(double milliseconds, int fps) =>
        FromSeconds(milliseconds / 1000.0, fps);

    /// <summary>
    ///     Returns new <see cref="FrameSpan" /> for <paramref name="milliseconds" />
    /// </summary>
    public static FrameSpan FromMilliseconds(double milliseconds) => FromSeconds(milliseconds / 1000.0);

    /// <summary>Returns the smaller of two <see cref="FrameSpan" />.</summary>
    public static FrameSpan Min(FrameSpan left, FrameSpan right) => left <= right ? left : right;

    /// <summary>Returns the larger of two <see cref="FrameSpan" />.</summary>
    public static FrameSpan Max(FrameSpan left, FrameSpan right) => left >= right ? left : right;

    /// <summary>
    ///     Returns the absolute value of a Frame.
    /// </summary>
    public static FrameSpan Abs(FrameSpan frame) => new(Math.Abs(frame.Frames));

    /// <summary>
    ///     Clamps frame value to a range
    /// </summary>
    public static FrameSpan Clamp(FrameSpan frame, int min, int max) => new(Math.Clamp(frame.Frames, min, max));

    /// <summary>
    ///     Clamps frame value to a range
    /// </summary>
    public static FrameSpan Clamp(FrameSpan frame, FrameSpan min, FrameSpan max) =>
        Clamp(frame, min.Frames, max.Frames);

    /// <summary>
    ///     Clamps frame value to a range
    /// </summary>
    public static FrameSpan Clamp(FrameSpan frame, Frame min, Frame max) =>
        Clamp(frame, min.Number, max.Number);

    /// <inheritdoc />
    public static bool operator >(FrameSpan left, FrameSpan right) => left.Frames > right.Frames;

    /// <inheritdoc />
    public static bool operator >=(FrameSpan left, FrameSpan right) => left.Frames >= right.Frames;

    /// <inheritdoc />
    public static bool operator <(FrameSpan left, FrameSpan right) => left.Frames < right.Frames;

    /// <inheritdoc />
    public static bool operator <=(FrameSpan left, FrameSpan right) => left.Frames <= right.Frames;

    /// <inheritdoc />
    public static FrameSpan operator %(FrameSpan left, int right) => new(left.Frames % right);

    /// <inheritdoc />
    public static FrameSpan operator +(FrameSpan left, int right) => new(left.Frames + right);

    /// <inheritdoc />
    public static FrameSpan operator -(FrameSpan left, int right) => new(left.Frames - right);

    /// <inheritdoc />
    public static FrameSpan operator *(FrameSpan left, int right) => new(left.Frames * right);

    /// <inheritdoc cref="op_Multiply(Backdash.FrameSpan,int)" />
    public static FrameSpan operator *(int left, FrameSpan right) => right * left;

    /// <inheritdoc />
    public static FrameSpan operator +(FrameSpan left, Frame right) => new(left.Frames + right.Number);

    /// <inheritdoc />
    public static FrameSpan operator -(FrameSpan left, Frame right) => new(left.Frames - right.Number);

    /// <inheritdoc />
    public static FrameSpan operator +(FrameSpan left, FrameSpan right) => new(left.Frames + right.Frames);

    /// <inheritdoc />
    public static FrameSpan operator -(FrameSpan left, FrameSpan right) => new(left.Frames - right.Frames);

    /// <inheritdoc />
    public static FrameRange operator +(FrameSpan left, FrameRange right) => right + left;

    /// <inheritdoc />
    public static FrameRange operator -(FrameSpan left, FrameRange right) => right + left;
}
