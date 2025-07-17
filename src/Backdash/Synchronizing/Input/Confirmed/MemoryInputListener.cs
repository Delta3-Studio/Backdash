using System.Collections;
using Backdash.Data;

namespace Backdash.Synchronizing.Input.Confirmed;

/// <summary>
///  Listener that saves the confirmed inputs in-memory
/// </summary>
public sealed class MemoryInputListener<TInput> : IInputListener<TInput>, IEnumerable<ConfirmedInputs<TInput>>
    where TInput : unmanaged
{
    readonly IInputListener<TInput>? nextListener;

    readonly List<ConfirmedInputs<TInput>> inputList;

    /// <summary>
    /// Returns all read inputs
    /// </summary>
    public IReadOnlyList<ConfirmedInputs<TInput>> Inputs { get; }

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
