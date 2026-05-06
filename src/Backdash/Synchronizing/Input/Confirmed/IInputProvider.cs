namespace Backdash.Synchronizing.Input.Confirmed;

/// <summary>
///     Provider for confirmed inputs
/// </summary>
public interface IInputProvider<TInput> : IDisposable where TInput : unmanaged
{
    /// <summary>
    ///     Get Inputs
    /// </summary>
    IReadOnlyList<ConfirmedInputs<TInput>> GetInputs(InputContext<TInput> context);

#pragma warning disable CA1816
    void IDisposable.Dispose() { }
#pragma warning restore CA1816
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

/// <summary>
///  Binary input provider
/// </summary>
/// <remarks>
/// Initialize input provider for a enumerable
/// </remarks>
public sealed class BinaryInputProvider<TInput> : IInputProvider<TInput> where TInput : unmanaged
{
    IReadOnlyList<ConfirmedInputs<TInput>>? inputs;
    MemoryStream? inputBytes;

    /// <summary>
    /// Initializes new binary input provider
    /// </summary>
    public BinaryInputProvider(ReadOnlySpan<byte> bytes) => inputBytes = InputCompressor<TInput>.DecompressStream(bytes);

    /// <inheritdoc />
    public IReadOnlyList<ConfirmedInputs<TInput>> GetInputs(InputContext<TInput> context)
    {
        if (inputs is not null) return inputs;
        if (inputBytes is null or { Length: 0 }) return [];
        inputs = InputCompressor<TInput>.Decompress(context, inputBytes);
        inputBytes?.Dispose();
        inputBytes = null;
        return inputs;
    }

    /// <inheritdoc />
    public void Dispose() => inputBytes?.Dispose();
}
