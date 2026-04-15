using Backdash.Data;

namespace Backdash.Synchronizing.State;

/// <summary>
///     A specific frame saved state snapshot.
/// </summary>
/// <param name="frame">Saved frame number</param>
/// <param name="state">Game state on <paramref name="frame" /></param>
/// <param name="checksum">Checksum of state</param>
[Serializable]
public sealed class StateSnapshot(Frame frame, uint checksum, byte[] state)
{
    /// <summary>
    /// Creates an empty state snapshot.
    /// </summary>
    public StateSnapshot() : this(Frame.Null, 0, []) { }

    /// <summary>Saved frame number</summary>
    public uint Checksum = checksum;

    /// <summary>Saved frame number</summary>
    public readonly Frame Frame = frame;

    /// <summary>Saved game state</summary>
    public readonly byte[] State = state;

    /// <summary>Saved state size</summary>
    public ByteSize Size => ByteSize.FromBytes(State.Length);
}
