using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backdash.JsonConverters;
using SpaceWar.Models;

namespace SpaceWar.Services;

public sealed class LobbyHttpClient(AppSettings appSettings)
{
    static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter(),
            new JsonIPAddressConverter(),
            new JsonIPEndPointConverter(),
        }
    };

    readonly HttpClient client = new()
    {
        BaseAddress = appSettings.ServerUrl,
    };

    readonly Guid recreationKey = Guid.NewGuid();

    public async Task<User> EnterLobby(
        string lobbyName, string username, PlayerMode mode,
        CancellationToken ct = default
    )
    {
        var localEndpoint = await GetLocalEndpoint(ct);
        var response = await client.PostAsJsonAsync($"/{appSettings.GameId}/lobby", new
        {
            lobbyName,
            username,
            mode,
            localEndpoint,
            recreationKey,
        }, jsonOptions, ct);

        if (response.StatusCode is HttpStatusCode.UnprocessableEntity)
            throw new InvalidOperationException("Failed to enter lobby");

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<User>(jsonOptions, ct)
                     ?? throw new InvalidOperationException();

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("token", result.Token.ToString());
        return result;
    }

    public async Task<Lobby?> GetLobby(CancellationToken ct = default)
    {
        var response = await client.GetAsync("/entry/lobby", ct);
        if (response.StatusCode is HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Lobby>(jsonOptions, ct);
    }

    public async Task LeaveLobby(CancellationToken ct = default)
    {
        var response = await client.DeleteAsync("/entry", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task ToggleReady(CancellationToken ct = default)
    {
        var response = await client.PutAsync("/entry/ready", null, ct);
        response.EnsureSuccessStatusCode();
    }

    async Task<IPEndPoint?> GetLocalEndpoint(CancellationToken ct = default)
    {
        try
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            await socket.ConnectAsync("8.8.8.8", 65530, ct);
            if (socket.LocalEndPoint is not IPEndPoint { Address: { } ipAddress })
                return null;

            return new(ipAddress, appSettings.LocalPort);
        }
        catch (Exception)
        {
            // skip
        }

        return null;
    }
}
