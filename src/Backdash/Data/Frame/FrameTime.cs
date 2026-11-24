using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

#pragma warning disable S3218, S4136

namespace Backdash;

/// <summary>
///     Frame time helpers
/// </summary>
public static class FrameTime
{
    /// <summary>Return one frame in seconds for 60 FPS</summary>
    public const float One60FpsF = 1f / 60f;

    /// <summary>Return one frame in seconds for 30 FPS</summary>
    public const float One30FpsF = 1f / 30f;

    /// <summary>Return one frame in seconds for 60 FPS</summary>
    public const double One60Fps = 1.0 / 60.0;

    /// <summary>Return one frame in seconds for 30 FPS</summary>
    public const double One30Fps = 1.0 / 30.0;

    /// <summary>
    ///     Default FPS (frames per second)
    ///     <value>60</value>
    /// </summary>
    public const int DefaultFrameRate = 60;

    /// <summary>
    /// Default Instance for <see cref="Fixed"/>
    /// </summary>
    public static readonly Fixed Default = new(DefaultFrameRate);

    /// <summary>
    ///     Current FPS (frames per second) used in <see cref="Default"/>. <br/>
    ///     Defaults to <see cref="DefaultFrameRate"/>
    /// </summary>
    public static int CurrentFrameRate
    {
        get => GetCurrentFrameRate();
        set => SetCurrentFrameRate(value);
    }

    /// <inheritdoc cref="Fixed.FrameSeconds"/>
    public static double FrameSeconds
    {
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Default.FrameSeconds;
    }

    /// <inheritdoc cref="Fixed.FrameTicks"/>
    public static long FrameTicks
    {
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Default.FrameTicks;
    }

    /// <inheritdoc cref="Fixed.FrameDuration"/>
    public static TimeSpan FrameDuration
    {
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Default.FrameDuration;
    }

    /// <inheritdoc cref="Fixed.FrameMilliseconds"/>
    public static double FrameMilliseconds
    {
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Default.FrameMilliseconds;
    }

    /// <inheritdoc cref="Fixed.FrameTimestamp"/>
    public static long FrameTimestamp
    {
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Default.FrameTimestamp;
    }

    /// <summary>
    /// Sets the current frame rate value.
    /// </summary>
    /// <seealso cref="CurrentFrameRate"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCurrentFrameRate(int newFps) => Default.SetFrameRate(newFps);

    /// <summary>
    /// Reads the current frame rate value.
    /// </summary>
    /// <seealso cref="CurrentFrameRate"/>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetCurrentFrameRate() => Default.FrameRate;

    /// <summary>
    ///     Returns <see cref="double" /> seconds for <paramref name="frameCount" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToSeconds(double frameCount, int fps) => frameCount / fps;

    /// <summary>
    ///     Returns <see cref="double" /> seconds for <paramref name="frameCount" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToSeconds(double frameCount) => Default.ToSeconds(frameCount);

    /// <summary>
    ///     Returns <see cref="double" /> seconds for <paramref name="frameCount" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToSeconds(int frameCount, int fps) => ToSeconds((double)frameCount, fps);

    /// <summary>
    ///     Returns <see cref="double" /> seconds for <paramref name="frameCount" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToSeconds(int frameCount) => Default.ToSeconds(frameCount);

    /// <summary>
    ///     Returns ticks for <paramref name="frameCount" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToTicks(int frameCount, int fps) => frameCount * TimeSpan.TicksPerSecond / fps;

    /// <summary>
    ///     Returns ticks for <paramref name="frameCount" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToTicks(int frameCount) => Default.ToTicks(frameCount);

    /// <summary>
    ///     Returns rounded ticks for <paramref name="frameCount" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToTicksRounded(int frameCount, int fps)
    {
        var ticks = frameCount * TimeSpan.TicksPerSecond;
        var quotient = Math.DivRem(ticks, fps, out long remainder);
        return quotient + (remainder > 0 ? 1 : 0);
    }

    /// <summary>
    ///     Returns <see cref="System.TimeSpan" /> for <paramref name="frameCount" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ToDuration(int frameCount, int fps) => TimeSpan.FromTicks(ToTicks(frameCount, fps));

    /// <summary>
    ///     Returns <see cref="System.TimeSpan" /> for <paramref name="frameCount" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ToDuration(int frameCount) => Default.ToDuration(frameCount);

    /// <summary>
    ///     Returns <see cref="double" /> milliseconds for <paramref name="frameCount" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToMilliseconds(double frameCount, int fps) => ToSeconds(frameCount, fps) * 1000.0;

    /// <summary>
    ///     Returns <see cref="double" /> milliseconds for <paramref name="frameCount" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToMilliseconds(double frameCount) => Default.ToMilliseconds(frameCount);

    /// <summary>
    ///     Returns <see cref="double" /> milliseconds for <paramref name="frameCount" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToMilliseconds(int frameCount, int fps) => ToMilliseconds((double)frameCount, fps);

    /// <summary>
    ///     Returns <see cref="double" /> milliseconds for <paramref name="frameCount" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToMilliseconds(int frameCount) => Default.ToMilliseconds(frameCount);

    /// <summary>
    ///     Returns the rounded <see cref="Stopwatch" /> timestamp for <paramref name="frame"/>.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToTimestamp(int frame, int fps)
    {
        var timestamp = frame * Stopwatch.Frequency;
        var quotient = Math.DivRem(timestamp, fps, out long remainder);
        return quotient + (remainder > 0 ? 1 : 0);
    }

    /// <summary>
    ///     Returns the <see cref="Stopwatch" /> timestamp for <paramref name="frame"/>.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToTimestamp(int frame) => Default.ToTimestamp(frame);

    /// <summary>
    ///     Returns the <see cref="Stopwatch" /> timestamp for the number of <paramref name="frames"/> in the future.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToTimestampDeadline(int frames, int fps) =>
        Stopwatch.GetTimestamp() + ToTimestamp(frames, fps);

    /// <summary>
    ///     Returns the <see cref="Stopwatch" /> timestamp for the number of <paramref name="frames"/> in the future.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ToTimestampDeadline(int frames) => Default.ToTimestampDeadline(frames);

    /// <summary>
    ///     Returns the number of frames for <paramref name="seconds" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromSeconds(double seconds, int fps) => (int)FromSecondsTotal(seconds, fps);

    /// <summary>
    ///     Returns the number of frames for <paramref name="seconds" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromSeconds(double seconds) => Default.FromSeconds(seconds);

    /// <summary>
    ///     Returns the number of frames for <paramref name="ticks" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromTicks(long ticks, int fps) => (int)FromTicksTotal(ticks, fps);

    /// <summary>
    ///     Returns the number of frames for <paramref name="ticks" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromTicks(long ticks) => Default.FromTicks(ticks);

    /// <summary>
    ///     Returns the number of frames for <paramref name="duration" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromDuration(TimeSpan duration, int fps) => (int)FromDurationTotal(duration, fps);

    /// <summary>
    ///     Returns the number of frames for <paramref name="duration" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromDuration(TimeSpan duration) => Default.FromDuration(duration);

    /// <summary>
    ///     Returns the number of frames for <paramref name="milliseconds" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromMilliseconds(double milliseconds, int fps) => (int)FromMillisecondsTotal(milliseconds, fps);

    /// <summary>
    ///     Returns the number of frames for <paramref name="milliseconds" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromMilliseconds(double milliseconds) => Default.FromMilliseconds(milliseconds);

    /// <summary>
    ///     Returns the number of frames elapsed for the specified
    ///     <see cref="Stopwatch"/> timestamp difference.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromTimestamp(long timestamp, int fps) =>
        (int)(timestamp * ((double)fps / Stopwatch.Frequency));

    /// <summary>
    ///     Returns the number of frames elapsed for the specified
    ///     <see cref="Stopwatch"/> timestamp difference.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromTimestamp(long timestamp) => Default.FromTimestamp(timestamp);

    /// <summary>
    ///     Returns the number of frames elapsed since the specified
    ///     <see cref="Stopwatch"/> timestamp.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromElapsedTimestamp(long timestamp, int fps) =>
        FromTimestamp(Stopwatch.GetTimestamp() - timestamp, fps);

    /// <summary>
    ///     Returns the number of frames elapsed since the specified
    ///     <see cref="Stopwatch"/> timestamp.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FromElapsedTimestamp(long timestamp) => Default.FromElapsedTimestamp(timestamp);

    /// <summary>
    ///     Returns the number of frames for <paramref name="seconds" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromSecondsTotal(double seconds, int fps) => seconds * fps;

    /// <summary>
    ///     Returns the number of frames for <paramref name="seconds" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromSecondsTotal(double seconds) => Default.FromSecondsTotal(seconds);

    /// <summary>
    ///     Returns the number of frames for <paramref name="ticks" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromTicksTotal(long ticks, int fps) => ticks * ((double)fps / TimeSpan.TicksPerSecond);

    /// <summary>
    ///     Returns the number of frames for <paramref name="ticks" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromTicksTotal(long ticks) => Default.FromTicksTotal(ticks);

    /// <summary>
    ///     Returns the number of frames for <paramref name="duration" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromDurationTotal(TimeSpan duration, int fps) => FromTicksTotal(duration.Ticks, fps);

    /// <summary>
    ///     Returns the number of frames for <paramref name="duration" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromDurationTotal(TimeSpan duration) => Default.FromDurationTotal(duration);

    /// <summary>
    ///     Returns the number of frames for <paramref name="milliseconds" /> at specified <paramref name="fps" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromMillisecondsTotal(double milliseconds, int fps) =>
        FromSecondsTotal(milliseconds / 1000.0, fps);

    /// <summary>
    ///     Returns the number of frames for <paramref name="milliseconds" />.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromMillisecondsTotal(double milliseconds) => Default.FromMillisecondsTotal(milliseconds);

    /// <summary>
    ///     Returns the total number of frames elapsed for the specified <see cref="Stopwatch"/> timestamp.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromTimestampTotal(long timestamp, int fps) => timestamp * ((double)fps / Stopwatch.Frequency);

    /// <summary>
    ///     Returns the total number of frames elapsed for the specified <see cref="Stopwatch"/> timestamp.
    /// </summary>
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromTimestampTotal(long timestamp) => Default.FromTimestampTotal(timestamp);

    /// <summary>
    ///     Returns the total number of frames elapsed since the specified <see cref="Stopwatch"/> timestamp.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromElapsedTimestampTotal(long timestamp, int fps) =>
        FromTimestampTotal(Stopwatch.GetTimestamp() - timestamp, fps);

    /// <summary>
    ///     Returns the total number of frames elapsed since the specified <see cref="Stopwatch"/> timestamp.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FromElapsedTimestampTotal(long timestamp) => Default.FromElapsedTimestampTotal(timestamp);

    /// <summary>
    ///     Frame time calculator
    /// </summary>
    [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
    public sealed class Fixed
    {
        int frameRate;

        /// <summary>
        ///     Frame time calculator
        /// </summary>
        public Fixed(int frameRate)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameRate);
            this.frameRate = frameRate;
        }

        /// <summary>
        ///     Used frame rate.
        /// </summary>
        public int FrameRate => frameRate;

        /// <summary>Return one frame in seconds</summary>
        public double FrameSeconds
        {
            [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ToSeconds(1);
        }

        /// <summary>Return one frame in ticks</summary>
        public long FrameTicks
        {
            [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ToTicks(1);
        }

        /// <summary>Return one frame duration</summary>
        public TimeSpan FrameDuration
        {
            [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ToDuration(1);
        }

        /// <summary>Return one frame in milliseconds</summary>
        public double FrameMilliseconds
        {
            [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ToMilliseconds(1);
        }

        /// <summary>Return one frame in <see cref="Stopwatch" /> timestamp</summary>
        public long FrameTimestamp
        {
            [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ToTimestamp(1);
        }

        /// <summary>
        /// Sets the current <see cref="FrameRate"/>
        /// </summary>
        public void SetFrameRate(int newFps)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newFps);
            frameRate = newFps;
        }

        /// <summary>
        ///     Returns <see cref="double" /> seconds for <paramref name="frameCount" /> .h
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ToSeconds(int frameCount) => FrameTime.ToSeconds(frameCount, frameRate);

        /// <summary>
        ///     Returns <see cref="double" /> seconds for <paramref name="frameCount" /> .h
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ToSeconds(double frameCount) => FrameTime.ToSeconds(frameCount, frameRate);

        /// <summary>
        ///     Returns ticks for <paramref name="frameCount" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ToTicks(int frameCount) => FrameTime.ToTicks(frameCount, frameRate);

        /// <summary>
        ///     Returns <see cref="System.TimeSpan" /> for <paramref name="frameCount" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan ToDuration(int frameCount) => FrameTime.ToDuration(frameCount, frameRate);

        /// <summary>
        ///     Returns <see cref="double" /> milliseconds for <paramref name="frameCount" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ToMilliseconds(int frameCount) => FrameTime.ToMilliseconds(frameCount, frameRate);

        /// <summary>
        ///     Returns <see cref="double" /> milliseconds for <paramref name="frameCount" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ToMilliseconds(double frameCount) => FrameTime.ToMilliseconds(frameCount, frameRate);

        /// <summary>
        ///     Returns the <see cref="Stopwatch" /> timestamp for frame <paramref name="frame"/>.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ToTimestamp(int frame) => FrameTime.ToTimestamp(frame, frameRate);

        /// <summary>
        ///     Returns the <see cref="Stopwatch" /> timestamp for frame <paramref name="frame"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ToTimestampDeadline(int frame) => FrameTime.ToTimestampDeadline(frame, frameRate);

        /// <summary>
        ///     Returns the number of frames for <paramref name="seconds" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FromSeconds(double seconds) => FrameTime.FromSeconds(seconds, frameRate);

        /// <summary>
        ///     Returns the number of frames for <paramref name="ticks" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FromTicks(long ticks) => FrameTime.FromTicks(ticks, frameRate);

        /// <summary>
        ///     Returns the number of frames for <paramref name="duration" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FromDuration(TimeSpan duration) => FrameTime.FromDuration(duration, frameRate);

        /// <inheritdoc cref="FrameTime.FromTimestamp(long, int)"/>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FromTimestamp(long timestamp) => FrameTime.FromTimestamp(timestamp, frameRate);

        /// <inheritdoc cref="FrameTime.FromElapsedTimestamp(long, int)"/>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FromElapsedTimestamp(long timestamp) => FrameTime.FromElapsedTimestamp(timestamp, frameRate);

        /// <summary>
        ///     Returns the number of frames for <paramref name="milliseconds" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FromMilliseconds(double milliseconds) =>
            FrameTime.FromMilliseconds(milliseconds, frameRate);

        /// <summary>
        ///     Returns the number of frames for <paramref name="milliseconds" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double FromMillisecondsTotal(double milliseconds) =>
            FrameTime.FromMillisecondsTotal(milliseconds, frameRate);

        /// <summary>
        ///     Returns the number of frames for <paramref name="seconds" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double FromSecondsTotal(double seconds) => FrameTime.FromSecondsTotal(seconds, frameRate);

        /// <summary>
        ///     Returns the number of frames for <paramref name="ticks" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double FromTicksTotal(long ticks) => FrameTime.FromTicksTotal(ticks, frameRate);

        /// <summary>
        ///     Returns the number of frames for <paramref name="duration" />.
        /// </summary>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double FromDurationTotal(TimeSpan duration) => FrameTime.FromDurationTotal(duration, frameRate);

        /// <inheritdoc cref="FrameTime.FromTimestampTotal(long, int)"/>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double FromTimestampTotal(long timestamp) => FrameTime.FromTimestampTotal(timestamp, frameRate);

        /// <inheritdoc cref="FrameTime.FromElapsedTimestampTotal(long, int)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double FromElapsedTimestampTotal(long timestamp) =>
            FrameTime.FromElapsedTimestampTotal(timestamp, frameRate);
    }
}

#pragma warning restore S3218, S4136
