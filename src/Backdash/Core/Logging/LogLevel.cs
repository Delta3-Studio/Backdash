namespace Backdash;

/// <summary>
///     Defines logging severity levels.
/// </summary>
public enum LogLevel : byte
{
    /// <summary>
    ///     Specifies that a logging category should not write any messages.
    /// </summary>
    None = byte.MaxValue,

    /// <summary>
    ///     Logs that contain the most detailed messages.
    /// </summary>
    Trace = 0,

    /// <summary>
    ///     Logs that are used for interactive investigation during development.
    /// </summary>
    Debug,

    /// <summary>
    ///     Logs that track the general flow of the application.
    /// </summary>
    Information,

    /// <summary>
    ///     Logs that highlight an abnormal or unexpected event in the application flow,
    ///     but do not otherwise cause the application execution to stop.
    /// </summary>
    Warning,

    /// <summary>
    ///     Logs that highlight when the current flow of execution is stopped due to a failure
    /// </summary>
    Error,
}
