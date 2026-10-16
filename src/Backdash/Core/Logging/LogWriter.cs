namespace Backdash.Core.Logging;

/// <summary>
///     Defines how and where the log should be written.
/// </summary>
public interface ILogWriter : IDisposable
{
    /// <summary>
    ///     Write <paramref name="chars" /> into an output.
    /// </summary>
    /// <param name="level">Current <see cref="LogLevel" /> level.</param>
    /// <param name="chars">Char buffer array containing the log message</param>
    /// <param name="size">Number of chars of characters to read from <paramref name="chars" /></param>
    void Write(LogLevel level, char[] chars, int size);
}

/// <summary>
///     Base implementation of <see cref="ILogWriter" /> for any <see cref="TextWriter" />.
/// </summary>
public abstract class TextLogWriter : ILogWriter
{
    /// <summary>
    ///     Current <see cref="System.IO.TextWriter" />
    /// </summary>
    protected abstract TextWriter TextWriter { get; }

    readonly Lock locker = new();
    bool disposed;

    /// <inheritdoc />
    public void Write(LogLevel level, char[] chars, int size)
    {
        lock (locker)
        {
            if (disposed) return;
            TextWriter.WriteLine(chars, 0, size);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (locker)
        {
            if (disposed) return;
            try
            {
                Dispose(true);
            }
            finally
            {
                disposed = true;
            }
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Releases all resources currently used by this <see cref="TextLogWriter" /> instance.
    /// </summary>
    /// <param name="disposing">
    ///     <see langword="true" /> if this method is being invoked by the <see cref="Dispose()" /> method,
    ///     otherwise <see langword="false" />.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            TextWriter.Dispose();
    }
}

sealed class DelegateLogWriter(Action<LogLevel, string> action) : ILogWriter
{
    readonly Lock locker = new();
    bool disposed;

    /// <inheritdoc />
    public void Write(LogLevel level, char[] chars, int size)
    {
        lock (locker)
        {
            if (disposed) return;
            action.Invoke(level, new(chars.AsSpan(0, size)));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (locker)
            disposed = true;
    }
}

sealed class EmptyLogWriter : ILogWriter
{
    /// <inheritdoc />
    public void Write(LogLevel level, char[] chars, int size) { }

    /// <inheritdoc />
    public void Dispose() { }
}

/// <summary>
///     Implementation of <see cref="ILogWriter" /> for logging into <see cref="Console" />.
/// </summary>
public sealed class ConsoleTextLogWriter : TextLogWriter
{
    /// <inheritdoc />
    protected override TextWriter TextWriter { get; } = Console.Out;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // Do not close Console.Out
    }
}

/// <summary>
///     Implementation of <see cref="ILogWriter" /> for logging into a file.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="FileTextLogWriter" /> class.
/// </remarks>
/// <param name="fileName">
///     Log file name
///     <remarks>
///         you can use placeholders for:
///         - <see cref="Environment.ProcessId" />: <c>"{{proc_id}}"</c>;
///         - <see cref="DateTime.UtcNow" />: <c>"{{timestamp}}"</c>;
///     </remarks>
///     <value>Defaults to <c>"{{proc_id}}_{{timestamp}}.log"</c></value>
/// </param>
/// <param name="append">
///     True to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter
///     has no effect, and the constructor creates a new file.
/// </param>
public sealed class FileTextLogWriter(string? fileName = null, bool append = true) : TextLogWriter
{
    /// <inheritdoc />
    protected override TextWriter TextWriter { get; } = GetLogFileWriter(fileName, append);

    const string DefaultFileName = "{{proc_id}}_{{timestamp}}.log";

    /// <summary>
    /// Return a log file stream writer
    /// </summary>
    /// <value><cons cref="DefaultFileName"/></value>
    public static StreamWriter GetLogFileWriter(string? fileName = null, bool append = true)
    {
        fileName = !string.IsNullOrWhiteSpace(fileName) ? fileName : DefaultFileName;

        if (Path.GetDirectoryName(fileName) is { } dir && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        fileName = fileName
            .Replace("{{proc_id}}", Environment.ProcessId.ToString())
            .Replace("{{timestamp}}", $"{DateTime.UtcNow:yyyyMMddhhmmss}");

        return new(fileName, append)
        {
            AutoFlush = true,
        };
    }
}
