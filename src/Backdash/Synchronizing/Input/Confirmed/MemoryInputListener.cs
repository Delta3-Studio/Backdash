using System.Collections;
using System.Runtime.InteropServices;
using Backdash.Data;

namespace Backdash.Synchronizing.Input.Confirmed;

/// <summary>
///  Listener that saves the confirmed inputs in-memory
/// </summary>
public sealed class MemoryInputListener<TInput> : IInputListener<TInput>, IInputCollection<TInput>
    where TInput : unmanaged
{
    readonly IInputListener<TInput>? nextListener;
    readonly List<ConfirmedInputs<TInput>> inputList;
    InputContext<TInput>? inputContext;

    /// <summary>
    /// Returns all read inputs
    /// </summary>
    public IReadOnlyList<ConfirmedInputs<TInput>> Inputs { get; }

    /// <summary>
    /// Returns the input count
    /// </summary>
    public int Count => inputList.Count;

    internal MemoryInputListener(IInputListener<TInput>? next)
    {
        inputList = new((int)ByteSize.FromKibiBytes(5).ByteCount);
        Inputs = inputList.AsReadOnly();
        nextListener = next;
    }

    /// <summary>
    /// initializes a memory input listener
    /// </summary>
    public MemoryInputListener() : this(null) { }

    /// <summary>
    /// Clear current inputs
    /// </summary>
    public void Clear() => inputList.Clear();

    /// <summary>
    /// Drop <paramref name="count"/> inputs
    /// </summary>
    public void Drop(int count)
    {
        if (count <= 0) return;
        var from = Count - count;

        if (from < 0)
        {
            Clear();
            return;
        }

        inputList.RemoveRange(from, Count - from);
    }

    /// <inheritdoc />
    public byte[] GetCompressedInputs() =>
        inputContext is null
            ? []
            : InputCompressor<TInput>.Compress(inputContext, CollectionsMarshal.AsSpan(inputList));

    /// <inheritdoc />
    public void OnConfirmed(in Frame frame, in ConfirmedInputs<TInput> inputs)
    {
        inputList.Add(inputs);
        nextListener?.OnConfirmed(in frame, inputs);
    }

    /// <inheritdoc />
    void IInputListener<TInput>.OnSessionStart(InputContext<TInput> context)
    {
        Clear();
        inputContext = context;
        nextListener?.OnSessionStart(context);
    }

    /// <inheritdoc />
    void IInputListener<TInput>.OnSessionClose() => nextListener?.OnSessionClose();

    /// <inheritdoc />
    public void Dispose() => nextListener?.Dispose();

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
    public List<ConfirmedInputs<TInput>>.Enumerator GetEnumerator() => inputList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    IEnumerator<ConfirmedInputs<TInput>> IEnumerable<ConfirmedInputs<TInput>>.GetEnumerator() => GetEnumerator();
}

/// <summary>
///  Provides in-memory confirmed inputs
/// </summary>
public interface IInputCollection<TInput> : IEnumerable<ConfirmedInputs<TInput>> where TInput : unmanaged
{
    /// <summary>
    /// Returns the input count
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Returns all read inputs
    /// </summary>
    IReadOnlyList<ConfirmedInputs<TInput>> Inputs { get; }

    /// <summary>
    /// Clear current inputs
    /// </summary>
    void Clear();

    /// <summary>
    /// Drop <paramref name="count"/> inputs
    /// </summary>
    void Drop(int count);

    /// <summary>
    /// Return all input bytes compressed with Deflate
    /// </summary>
    byte[] GetCompressedInputs();
}
