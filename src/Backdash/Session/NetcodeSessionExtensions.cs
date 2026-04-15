using Backdash.Serialization;
using Backdash.Synchronizing.State;

namespace Backdash;

/// <summary>
/// Extensions for <see cref="INetcodeSession{T}"/>
/// </summary>
public static class NetcodeSessionExtensions
{
    /// <summary>
    ///    Returns the current state string representation
    /// </summary>
    public static StatePreview GetStateString<T>(this INetcodeSession<T> @this, IStateStringParser? parser = null)
        where T : unmanaged
    {
        var state = @this.GetSavedState();
        var currentBytes = state.GameState.WrittenSpan;
        var text = @this.GetStateString(state.Frame, currentBytes, parser);
        return new(state.Frame, state.Checksum, text);
    }

    /// <summary>
    ///    Returns string representation for given <paramref name="state"/>
    /// </summary>
    public static StatePreview GetStateString<T>(
        this INetcodeSession<T> @this,
        StateSnapshot state,
        IStateStringParser? parser = null
    ) where T : unmanaged
    {
        var stateBytes = state.State.AsSpan(0, (int)state.Size);
        var text = @this.GetStateString(state.Frame, stateBytes, parser);
        return new(state.Frame, state.Checksum, text);
    }

    /// <summary>
    ///    Returns string representation for given <paramref name="frame"/> and <paramref name="stateBytes"/>
    /// </summary>
    static string GetStateString<T>(
        this INetcodeSession<T> @this,
        Frame frame,
        ReadOnlySpan<byte> stateBytes,
        IStateStringParser? parser = null
    ) where T : unmanaged
    {
        parser ??= JsonStateStringParser.Singleton;
        var offset = 0;
        BinaryBufferReader reader = new(stateBytes, ref offset, @this.StateSerializationEndianness);
        var stateObject = @this.GetHandler().CreateState(frame, ref reader);
        return parser.GetStateString(frame, in reader, stateObject);
    }

    /// <summary>
    ///    Enumerate all valid state saved snapshots in descending order
    /// </summary>
    public static IEnumerable<StateSnapshot> EnumerateSnapshots<T>(this INetcodeSession<T> @this, Frame? frame = null)
        where T : unmanaged
    {
        StateSnapshot? next = frame.HasValue ? @this.GetStateSnapshot(frame.Value) : @this.GetStateSnapshot();
        while (next is not null)
        {
            yield return next;
            next = @this.GetStateSnapshot(next.Frame.Previous());
        }
    }

    /// <summary>
    ///    Enumerate string representation for all saved states in descending order
    /// </summary>
    public static IEnumerable<StatePreview> EnumerateStateStrings<T>(
        this INetcodeSession<T> @this, Frame? frame = null, IStateStringParser? parser = null)
        where T : unmanaged =>
        @this.EnumerateSnapshots(frame).Select(s => @this.GetStateString(s, parser));
}
