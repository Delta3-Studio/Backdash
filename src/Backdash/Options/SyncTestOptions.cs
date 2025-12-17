using System.Text.Json;
using Backdash.Synchronizing.Input;
using Backdash.Synchronizing.State;

namespace Backdash.Options;

/// <summary>
///     Configurations for <see cref="INetcodeSession{TInput}" /> in <see cref="SessionMode.SyncTest" /> mode.
/// </summary>
public sealed record SyncTestOptions<TInput> where TInput : unmanaged
{
    /// <summary>
    ///     Total forced rollback frames.
    /// </summary>
    /// <value>Defaults to <c>1</c></value>
    public int CheckDistanceFrames { get; set; } = 1;

    /// <summary>
    ///     If true, throws on state de-synchronization.
    /// </summary>
    public bool ThrowOnDesync { get; set; } = true;

    /// <summary>
    ///     If true, log state string on desync
    /// </summary>
    public bool LogStateOnDesync { get; set; } = true;

    /// <summary>
    ///     Sets desync handler for <see cref="SessionMode.SyncTest" /> sessions.
    ///     Useful for showing smart state diff.
    /// </summary>
    public IStateDesyncHandler? DesyncHandler { get; set; }

    /// <summary>
    ///     Sets desync handler for <see cref="SessionMode.SyncTest" /> sessions.
    ///     Useful for showing smart state diff.
    /// </summary>
    public IStateStringParser? StateStringParser { get; set; }

    /// <summary>
    ///     Input generator service for session.
    /// </summary>
    public IInputGenerator<TInput>? InputProvider { get; set; }

    /// <inheritdoc cref="CheckDistanceFrames" />
    public SyncTestOptions<TInput> CheckDistance(int frames)
    {
        CheckDistanceFrames = frames;
        return this;
    }

    /// <inheritdoc cref="LogStateOnDesync" />
    public SyncTestOptions<TInput> LogState(bool enabled = true)
    {
        LogStateOnDesync = enabled;
        return this;
    }

    /// <inheritdoc cref="ThrowOnDesync" />
    public SyncTestOptions<TInput> ThrowError(bool enabled = true)
    {
        ThrowOnDesync = enabled;
        return this;
    }

    /// <summary>
    ///     Use <see cref="RandomInputGenerator{TInput}" /> as input provider.
    /// </summary>
    /// <seealso cref="InputProvider" />
    public SyncTestOptions<TInput> UseRandomInputProvider()
    {
        InputProvider = new RandomInputGenerator<TInput>();
        return this;
    }

    /// <summary>
    ///     Use <see cref="JsonStateStringParser" /> as state viewer.
    /// </summary>
    public SyncTestOptions<TInput> UseJsonStateParser(JsonSerializerOptions? options = null)
    {
        StateStringParser = new JsonStateStringParser(options);
        return this;
    }

    /// <inheritdoc cref="UseJsonStateParser(JsonSerializerOptions?)" />
    public SyncTestOptions<TInput> UseJsonStateParser(Action<JsonSerializerOptions> configure)
    {
        var options = JsonStateStringParser.CreateDefaultJsonOptions();
        configure.Invoke(options);
        return UseJsonStateParser(options);
    }

    /// <inheritdoc cref="StateStringParser" />
    public SyncTestOptions<TInput> UseStateStringParser<T>() where T : IStateStringParser, new()
    {
        StateStringParser = new T();
        return this;
    }

    /// <inheritdoc cref="DesyncHandler" />
    public SyncTestOptions<TInput> UseDesyncHandler<T>() where T : IStateDesyncHandler, new()
    {
        DesyncHandler = new T();
        return this;
    }

    /// <inheritdoc cref="InputProvider" />
    public SyncTestOptions<TInput> UseInputProvider<T>() where T : IInputGenerator<TInput>, new()
    {
        InputProvider = new T();
        return this;
    }
}
