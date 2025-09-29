using Backdash.Data;

namespace Backdash.Synchronizing.State;

/// <summary>
///     A specific frame saved state snapshot.
/// </summary>
/// <param name="frame">Saved frame number</param>
/// <param name="state">Game state on <paramref name="frame" /></param>
[Serializable]
public sealed class StateSnapshot(Frame frame, byte[] state)
{
    /// <summary>
    /// Creates an empty state snapshot.
    /// </summary>
    public StateSnapshot() : this(Frame.Null, []) { }

    /// <summary>Saved frame number</summary>
    public Frame Frame = frame;

    /// <summary>Saved game state</summary>
    public byte[] State = state;

    /// <summary>Saved state size</summary>
    public ByteSize Size => ByteSize.FromBytes(State.Length);
}
