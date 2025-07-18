using System.Collections.Immutable;
using System.IO.Compression;

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
    ImmutableArray<ConfirmedInputs<TInput>>? inputs;
    MemoryStream? inputBytes;

    /// <summary>
    /// Initializes new binary input provider
    /// </summary>
    public BinaryInputProvider(ReadOnlySpan<byte> bytes)
    {
        inputBytes = new();
        unsafe
        {
            fixed (byte* ptr = bytes)
            {
                UnmanagedMemoryStream stream = new(ptr, bytes.Length);
                using DeflateStream decompressor = new(stream, CompressionMode.Decompress);
                decompressor.CopyTo(inputBytes);
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<ConfirmedInputs<TInput>> GetInputs(InputContext<TInput> context)
    {
        if (inputs is not null) return inputs;
        if (inputBytes is null) return [];

        var buffer = new byte[context.ConfirmedInputSize];
        List<ConfirmedInputs<TInput>> result = [];
        ConfirmedInputs<TInput> confirmedInput = new();

        inputBytes.Seek(0, SeekOrigin.Begin);
        while (inputBytes.Read(buffer) > 0)
        {
            context.Read(buffer, ref confirmedInput);
            result.Add(confirmedInput);
        }

        inputs = [.. result];

        inputBytes?.Dispose();
        inputBytes = null;

        return inputs;
    }

    /// <inheritdoc />
    public void Dispose() => inputBytes?.Dispose();
}
