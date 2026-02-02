using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace SpaceWar.Models;

public sealed class Peer
{
    public required Guid PeerId { get; init; }
    public required string Username { get; init; }
    public required IPEndPoint Endpoint { get; init; }
    public IPEndPoint? LocalEndpoint { get; init; }
    public bool Connected { get; init; }
    public bool Ready { get; init; }

    [MemberNotNullWhen(true, nameof(LocalEndpoint))]
    public bool InSameNetwork(User user) => LocalEndpoint is not null && Equals(Endpoint.Address, user.IP);

    public IPEndPoint GetEndpointForUser(User user) => InSameNetwork(user) ? LocalEndpoint : Endpoint;
}
