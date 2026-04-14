using Backdash.Core;

namespace Backdash.Network.Protocol;

enum ProtocolEvent : byte
{
    Connected,
    Synchronizing,
    Synchronized,
    SyncFailure,
    Disconnected,
    NetworkInterrupted,
    NetworkResumed,
}

struct ProtocolEventInfo(ProtocolEvent type, NetcodePlayer player) : IUtf8SpanFormattable
{
    public readonly ProtocolEvent Type = type;
    public readonly NetcodePlayer Player = player;
    public SynchronizingEventInfo Synchronizing = default;
    public SynchronizedEventInfo Synchronized = default;
    public ConnectionInterruptedEventInfo NetworkInterrupted = default;

    public readonly bool TryFormat(
        Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        bytesWritten = 0;
        Utf8StringBuilder writer = new(in utf8Destination, ref bytesWritten);
        if (!writer.Write("P"u8)) return false;
        if (!writer.Write(Player.Index)) return false;
        if (!writer.Write(" ProtoEvt "u8)) return false;
        if (!writer.WriteEnum(Type)) return false;
        if (!writer.Write(":"u8)) return false;
        return Type switch
        {
            ProtocolEvent.NetworkInterrupted => writer.Write("Timeout: "u8) &&
                                                writer.Write(NetworkInterrupted.DisconnectTimeout),
            ProtocolEvent.Synchronizing when !writer.Write(' ') => false,
            ProtocolEvent.Synchronizing => writer.Write(Synchronizing.CurrentStep) && writer.Write('/') &&
                                           writer.Write(Synchronizing.TotalSteps),
            _ => writer.Write("{}"u8),
        };
    }
}
