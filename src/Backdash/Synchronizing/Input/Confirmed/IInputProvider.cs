namespace Backdash.Synchronizing.Input.Confirmed;

/// <summary>
///     Provider for confirmed inputs
/// </summary>
public interface IInputProvider<TInput> where TInput : unmanaged
{
    /// <summary>
    ///     Session Started
    /// </summary>
    IReadOnlyList<ConfirmedInputs<TInput>> GetInputs(InputContext<TInput> context);
}

/// <summary>
///  Enumerable input provider
/// </summary>
/// <remarks>
/// Initialize input provider for a enumerable
/// </remarks>
public sealed class EnumerableInputProvider<TInput>(IEnumerable<ConfirmedInputs<TInput>> inputSeq)
    : IInputProvider<TInput> where TInput : unmanaged
{
    readonly Lazy<IReadOnlyList<ConfirmedInputs<TInput>>> inputs = new(() => [.. inputSeq]);

    /// <inheritdoc />
    public IReadOnlyList<ConfirmedInputs<TInput>> GetInputs(InputContext<TInput> context) => inputs.Value;
}
