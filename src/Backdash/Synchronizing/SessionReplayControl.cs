namespace Backdash.Synchronizing;

/// <summary>
///     Control flow of a <see cref="SessionMode.Replay" /> session.
///     <seealso cref="NetcodeSessionBuilder{TInput}.ForReplay" />
/// </summary>
public class SessionReplayControl
{
    /// <summary>
    ///     Maximum number of frames for backward play on Replays
    ///     Defaults to 300 (5 seconds in 60 fps)
    /// </summary>
    public FrameSpan MaxBackwardFrames { get; init; } = FrameSpan.FromSeconds(5);

    /// <summary>
    ///     Last session recorded input frame
    /// </summary>
    public Frame LastInputFrame { get; internal set; } = Frame.Zero;

    /// <summary>
    ///     true if replay will flow backwards
    /// </summary>
    public bool IsBackward { get; set; }

    /// <summary>
    ///     true if replay is paused
    /// </summary>
    public bool IsPaused { get; private set; }

    /// <summary>
    ///     Pause replay. <seealso cref="IsPaused" />
    /// </summary>
    public void Pause() => IsPaused = true;

    /// <summary>
    ///     Toggle replay pause. <seealso cref="IsPaused" />
    /// </summary>
    public void TogglePause() => IsPaused = !IsPaused;

    /// <summary>
    ///     Toggle replay backward. <seealso cref="IsBackward" />
    /// </summary>
    public void ToggleBackwards() => IsBackward = !IsBackward;

    /// <summary>
    ///     Unpause state <seealso cref="IsPaused" />
    /// </summary>
    public void Play(bool isBackwards = false)
    {
        IsPaused = false;
        IsBackward = isBackwards;
    }
}
