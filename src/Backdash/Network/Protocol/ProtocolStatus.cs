namespace Backdash.Network.Protocol;

enum ProtocolStatus
{
    Syncing,
    Running,
    Disconnecting,
    Disconnected,
}

static class ProtocolStatusEx
{
    public static PlayerConnectionStatus ToPlayerStatus(this ProtocolStatus status) => status switch
    {
        ProtocolStatus.Syncing => PlayerConnectionStatus.Syncing,
        ProtocolStatus.Running => PlayerConnectionStatus.Connected,
        ProtocolStatus.Disconnected => PlayerConnectionStatus.Disconnected,
        _ => PlayerConnectionStatus.Unknown,
    };
}
