using Backdash.Synchronizing;
using Backdash.Synchronizing.Input.Confirmed;

namespace Backdash.Options;

/// <summary>
///     Configurations for <see cref="INetcodeSession{TInput}" /> in <see cref="SessionMode.Replay" /> mode.
/// </summary>
public sealed record SessionReplayOptions<TInput> where TInput : unmanaged
{
    /// <summary>
    ///     Controller for replay session.
    /// </summary>
    public SessionReplayControl? ReplayController { get; set; }

    /// <summary>
    ///     Inputs to be replayed
    /// </summary>
    public IInputProvider<TInput>? InputProvider { get; set; }


    /// <inheritdoc cref="IInputProvider{TInput}" />
    public SessionReplayOptions<TInput> UseInputProvider<T>() where T : IInputProvider<TInput>, new()
    {
        InputProvider = new T();
        return this;
    }

    /// <inheritdoc cref="IInputProvider{TInput}" />
    public SessionReplayOptions<TInput> WithInputProvider(IInputProvider<TInput> provider)
    {
        InputProvider = provider;
        return this;
    }

    /// <inheritdoc cref="IInputProvider{TInput}" />
    public SessionReplayOptions<TInput> WithInputs(IEnumerable<ConfirmedInputs<TInput>> inputs) =>
        WithInputProvider(new EnumerableInputProvider<TInput>(inputs));

    /// <inheritdoc cref="IInputProvider{TInput}" />
    public SessionReplayOptions<TInput> WithInputs(ReadOnlySpan<byte> inputs) =>
        WithInputProvider(new BinaryInputProvider<TInput>(inputs));

    /// <inheritdoc cref="IInputProvider{TInput}" />
    public SessionReplayOptions<TInput> WithInputsFile(string inputFile)
    {
        if (!File.Exists(inputFile))
            throw new InvalidOperationException("Invalid input file");

        var inputs = File.ReadAllBytes(inputFile);
        return WithInputs(inputs);
    }
}
