using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backdash.Serialization;

namespace Backdash.Synchronizing.State;

/// <summary>
///     Binary store for temporary save and restore game states using <see cref="IBinarySerializer{T}" />.
/// </summary>
/// <param name="hintSize">initial memory used for infer the state size</param>
public sealed class DefaultStateStore(int hintSize) : IStateStore
{
    int head;
    SavedState[] savedStates = [];

    /// <inheritdoc />
    public void Initialize(int saveCount)
    {
        savedStates = new SavedState[saveCount];
        for (var i = 0; i < saveCount; i++)
            savedStates[i] = new(Frame.Null, new(hintSize), 0);
    }

    /// <inheritdoc />
    public ref SavedState Next()
    {
        ref var result = ref savedStates[head];
        result.GameState.ResetWrittenCount();
        return ref result!;
    }

    /// <inheritdoc />
    public bool TryLoad(Frame frame, [MaybeNullWhen(false)] out SavedState result)
    {
        var i = 0;
        ref var current = ref MemoryMarshal.GetReference(savedStates.AsSpan());
        ref var limit = ref Unsafe.Add(ref current, savedStates.Length);
        while (Unsafe.IsAddressLessThan(ref current, ref limit))
        {
            if (current.Frame.Number == frame.Number)
            {
                head = i;
                Advance();
                result = current;
                return true;
            }

            i++;
            current = ref Unsafe.Subtract(ref current, 1)!;
        }

        result = null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGet(Frame frame, [MaybeNullWhen(false)] out SavedState result)
    {
        ref var current = ref MemoryMarshal.GetReference(savedStates.AsSpan());
        ref var limit = ref Unsafe.Add(ref current, savedStates.Length);
        while (Unsafe.IsAddressLessThan(ref current, ref limit))
        {
            if (current.Frame.Number == frame.Number)
            {
                result = current;
                return true;
            }

            current = ref Unsafe.Subtract(ref current, 1)!;
        }

        result = null;
        return false;
    }

    /// <inheritdoc />
    public SavedState Last()
    {
        var i = head - 1;
        var index = i < 0 ? savedStates.Length - 1 : i;
        return savedStates[index];
    }

    /// <inheritdoc />
    public void Advance() => head = (head + 1) % savedStates.Length;
}
