using System.Net;
using Backdash;
using Backdash.Core;
using SpaceWar;
using SpaceWar.Logic;

var session = ParseSessionArgs(args);

using var game = new Game1(session);
game.Run();

return;

static INetcodeSession<PlayerInputs> ParseSessionArgs(string[] args)
{
    if (args is not [{ } portArg, { } playerCountArg, .. { } lastArgs]
        || !int.TryParse(portArg, out var port)
        || !int.TryParse(playerCountArg, out var playerCount))
        throw new InvalidOperationException("Invalid port argument");

    if (playerCount > Config.MaxShips)
        throw new InvalidOperationException("Too many players");

    // create rollback session builder
    var builder = RollbackNetcode
        .WithInputType<PlayerInputs>()
        .WithPort(port)
        .WithPlayerCount(playerCount)
        .WithInputDelayFrames(2)
        .WithLogLevel(LogLevel.Information)
        .WithConfirmedInputHistory()
        .ConfigureProtocol(options =>
        {
            options.NumberOfSyncRoundTrips = 10;
            options.DisconnectTimeout = TimeSpan.FromSeconds(5);
            options.DisconnectNotifyStart = TimeSpan.FromSeconds(2);
            options.NetworkPackageStatsEnabled = false;
            // options.NetworkLatency = Backdash.Data.FrameSpan.Of(3).Duration();
        });

    switch (lastArgs)
    {
        case ["local-only", ..]:
            return builder
                .WithSaveStateCount(120)
                .ForLocal()
                .Build();

        case ["spectate", { } hostArg] when IPEndPoint.TryParse(hostArg, out var hostEndpoint):
            return builder
                .WithFileLogWriter($"logs/log_spectator_{port}.log", append: false)
                .ForSpectator(hostEndpoint)
                .Build();

        case ["replay", { } replayFile]:
            return builder
                .ForReplay(options => options.WithInputsFile(replayFile))
                .Build();

        case ["sync-test-auto", ..]:
            return builder
                .ForSyncTest(options => options
                    .UseJsonStateParser()
                    .UseDesyncHandler<DiffPlexDesyncHandler>()
                    .UseRandomInputProvider()
                )
                .Build();

        case ["sync-test", ..]:
            return builder
                .ForSyncTest(options => options
                    .UseJsonStateParser()
                    .UseDesyncHandler<DiffPlexDesyncHandler>()
                )
                .Build();

        default:
            // defaults to remote session
            var players = lastArgs.Select(ParsePlayer).ToArray();

            if (!players.Any(x => x.IsLocal()))
                throw new InvalidOperationException("No local player defined");

            return builder
                .WithFileLogWriter($"logs/log_player_{port}.log", append: false)
                .WithPlayers(players)
                .ForRemote()
                .Build();
    }
}

static NetcodePlayer ParsePlayer(string address)
{
    if (address.Equals("local", StringComparison.OrdinalIgnoreCase))
        return NetcodePlayer.CreateLocal();

    if (address.StartsWith("s:", StringComparison.OrdinalIgnoreCase))
        if (IPEndPoint.TryParse(address[2..], out var hostEndPoint))
            return NetcodePlayer.CreateSpectator(hostEndPoint);
        else
            throw new InvalidOperationException("Invalid spectator endpoint");

    if (IPEndPoint.TryParse(address, out var endPoint))
    {
        return NetcodePlayer.CreateRemote(endPoint);
    }

    throw new InvalidOperationException($"Invalid player argument: {address}");
}
