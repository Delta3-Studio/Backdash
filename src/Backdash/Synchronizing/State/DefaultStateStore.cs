// ReSharper disable ForCanBeConvertedToForeach, RedundantSuppressNullableWarningExpression

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Backdash.Serialization;

namespace Backdash.Synchronizing.State;

/// <summary>
///     Binary store for temporary save and restore game states using <see cref="IBinarySerializer{T}" />.
/// </summary>
/// <param name="hintSize">initial memory used to infer the state size</param>
public sealed class DefaultStateStore(int hintSize) : IStateStore
{
    int head;
    SavedState[] savedStates = [];

    /// <inheritdoc />
    public void Initialize(int saveCount)
    {
        savedStates = new SavedState[saveCount];
        for (var i = 0; i < savedStates.Length; i++)
            savedStates[i] = new(hintSize);
    }

    /// <inheritdoc />
    public void Advance() => head = (head + 1) % savedStates.Length;

    /// <inheritdoc />
    public void Clear()
    {
        head = 0;
        foreach (var state in savedStates)
            state.Clear();
    }

    /// <inheritdoc />
    public ref SavedState Next()
    {
        ref var result = ref savedStates[head];
        result.GameState.ResetWrittenCount();
        return ref result!;
    }

    int LastIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (head is 0 ? savedStates.Length : head) - 1;
    }

    /// <inheritdoc />
    public ref SavedState Last() => ref savedStates[LastIndex];

    const int NotFound = -1;

    int FindIndex(Frame frame)
    {
        if (frame.IsNull) return NotFound;
        ref var last = ref Last();
        var lastFrame = last.Frame;
        var delta = lastFrame.Number - frame.Number;
        if (delta < 0 || delta >= savedStates.Length)
            return NotFound;

        var slot = LastIndex - delta;
        if (slot < 0) slot += savedStates.Length;
        if (savedStates[slot].Frame.Number == frame.Number)
            return slot;

        var index = head;
        var span = savedStates.AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            if (--index < 0) index = span.Length - 1;
            if (span[index].Frame == frame)
                return index;
        }

        return NotFound;
    }

    /// <inheritdoc />
    public bool Seek(Frame frame)
    {
        var index = FindIndex(frame);
        if (index < 0) return false;
        head = index;
        return true;
    }

    /// <inheritdoc />
    public bool TryLoad(Frame frame, [MaybeNullWhen(false)] out SavedState result)
    {
        if (Seek(frame))
        {
            result = savedStates[head];
            Advance();
            return true;
        }

        result = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGet(Frame frame, [MaybeNullWhen(false)] out SavedState result)
    {
        var index = FindIndex(frame);
        if (index < 0)
        {
            result = null;
            return false;
        }

        result = savedStates[index];
        return true;
    }
}
