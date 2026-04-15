using Backdash.Data;

namespace Backdash.Synchronizing.State;

/// <summary>
///     A specific frame saved state snapshot.
/// </summary>
/// <param name="frame">Saved frame number</param>
/// <param name="state">Game state on <paramref name="frame" /></param>
/// <param name="checksum">Checksum of state</param>
[Serializable]
public sealed class StateSnapshot(Frame frame, Checksum checksum, byte[] state)
{
    /// <summary>
    /// Creates an empty state snapshot.
    /// </summary>
    public StateSnapshot() : this(Frame.Null, Checksum.Empty, []) { }

    /// <summary>Saved frame number</summary>
    public Frame Frame = frame;

    /// <summary>Saved state checksum</summary>
    public readonly Checksum Checksum = checksum;

    /// <summary>Saved game state</summary>
    public readonly byte[] State = state;

    /// <summary>Saved state size</summary>
    public ByteSize Size => ByteSize.FromBytes(State.Length);
}
