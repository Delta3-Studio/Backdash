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
    public static string GetStateString<T>(
        this INetcodeSession<T> @this,
        IStateStringParser? parser = null
    ) where T : unmanaged =>
        @this.GetStateString(@this.GetCurrentSavedFrame(), parser);

    /// <summary>
    ///    Returns string representation for given <paramref name="state"/>
    /// </summary>
    public static string GetStateString<T>(
        this INetcodeSession<T> @this,
        SavedFrame state,
        IStateStringParser? parser = null
    ) where T : unmanaged
    {
        parser ??= JsonStateStringParser.Singleton;
        var currentOffset = 0;
        var currentBytes = state.GameState.WrittenSpan;
        BinaryBufferReader currentReader = new(currentBytes, ref currentOffset, @this.StateSerializationEndianness);
        var currentObject = @this.GetHandler().CreateState(state.Frame, ref currentReader);
        return parser.GetStateString(state.Frame, in currentReader, currentObject);
    }

    /// <summary>
    ///     Returns the last saved state snapshot.
    /// </summary>
    public static StateSnapshot CurrentStateSnapshot<T>(this INetcodeSession<T> @this) where T : unmanaged =>
        @this.GetCurrentSavedFrame().ToSnapshot();
}
