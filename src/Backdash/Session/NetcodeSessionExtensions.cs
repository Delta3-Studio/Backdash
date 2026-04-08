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
    public static string GetStateString<T>(this INetcodeSession<T> @this, IStateStringParser? parser = null)
        where T : unmanaged
    {
        var state = @this.GetCurrentSavedFrame();
        var currentBytes = state.GameState.WrittenSpan;
        return @this.GetStateString(state.Frame, currentBytes, parser);
    }

    /// <summary>
    ///    Returns string representation for given <paramref name="state"/>
    /// </summary>
    public static string GetStateString<T>(
        this INetcodeSession<T> @this,
        StateSnapshot state,
        IStateStringParser? parser = null
    ) where T : unmanaged
    {
        var stateBytes = state.State.AsSpan(0, (int)state.Size);
        return @this.GetStateString(state.Frame, stateBytes, parser);
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
}
