using Backdash.Serialization;

namespace Backdash.Synchronizing.State;

/// <summary>
///     Handles <see cref="SessionMode.SyncTest" /> state desync.
/// </summary>
public interface IStateDesyncHandler
{
    /// <summary>
    ///     Handles the states binary representations
    /// </summary>
    void Handle(INetcodeSession session, in DesyncState previous, in DesyncState current);
}

/// <summary>
///  State desync snapshot
/// </summary>
public readonly ref struct DesyncState(
    string value,
    ref readonly BinaryBufferReader reader,
    uint checksum,
    object? state
)
{
    /// <summary>State text representation</summary>
    public readonly string Value = value;

    /// <summary>State object value</summary>
    /// <seealso cref="INetcodeSessionHandler.CreateState"/>
    public readonly object? State = state;

    /// <summary>State binary reader</summary>
    public readonly BinaryBufferReader Reader = reader;

    /// <summary>State checksum value</summary>
    public readonly uint Checksum = checksum;

    /// <inheritdoc />
    public override string ToString() => Value;
}
