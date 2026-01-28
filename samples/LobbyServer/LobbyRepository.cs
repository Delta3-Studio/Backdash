using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LobbyServer;

public sealed class LobbyRepository(
    IMemoryCache cache,
    TimeProvider time,
    IOptions<AppSettings> settings
)
{
    public EnterLobbyResponse? EnterOrCreate(IPAddress remote, EnterLobbyRequest req)
    {
        var lobbyName = req.LobbyName.NormalizeName();
        var lobbyKey = MountLobbyKey(lobbyName);
        var userName = req.Username.NormalizeName();
        var expiration = settings.Value.LobbyExpiration;
        var peerId = Guid.NewGuid();
        var now = time.GetUtcNow();

        var lobby = cache.GetOrCreate(lobbyKey, e =>
        {
            e.SetSlidingExpiration(expiration);
            return new Lobby(
                key: lobbyKey,
                name: lobbyName,
                owner: peerId,
                expiration: expiration,
                purgeTimeout: settings.Value.PurgeTimeout,
                createdAt: now,
                maxPlayers: req.MaxPlayers
            );
        });

        if (lobby is null || lobby.Ready)
            return null;

        lock (lobby.Locker)
        {
            var userNameIndex = 2;
            var nextUserName = userName;
            while (lobby.FindEntry(nextUserName) is not null)
                nextUserName = $"{userName}{userNameIndex++}";
            userName = nextUserName;

            Peer peer = new(userName, remote)
            {
                PeerId = peerId,
                LocalEndpoint = req.LocalEndpoint,
            };

            LobbyEntry entry = new(peer, req.Mode)
            {
                LastRead = now,
            };

            using var playerEntry = cache.CreateEntry(entry.Token);
            playerEntry.Value = entry;
            playerEntry.SetSlidingExpiration(expiration);
            entry = lobby.AddPeer(entry);
            return new(userName, lobbyName, entry.Mode, entry.Peer.PeerId, entry.Token, remote);
        }
    }

    public Lobby? FindLobby(string name)
    {
        var key = MountLobbyKey(name.NormalizeName());
        return cache.Get<Lobby>(key);
    }

    static string MountLobbyKey(string name) => name.WithPrefix("lobby_");

    public LobbyEntry? FindEntry(Guid peerToken)
    {
        if (!cache.TryGetValue<LobbyEntry>(peerToken, out var entry) || entry is null)
            return null;

        var now = time.GetUtcNow();
        entry.LastRead = now;

        return entry;
    }

    public void Remove(Lobby lobby) => cache.Remove(lobby.Key);
}
