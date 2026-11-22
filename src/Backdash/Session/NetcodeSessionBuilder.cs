using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using Backdash.Backends;
using Backdash.Core;
using Backdash.Data;
using Backdash.Network;
using Backdash.Network.Client;
using Backdash.Options;
using Backdash.Serialization;
using Backdash.Serialization.Internal;
using Backdash.Synchronizing.Input.Confirmed;
using Backdash.Synchronizing.Random;
using Backdash.Synchronizing.State;

// ReSharper disable LocalVariableHidesMember, ParameterHidesMember
#pragma warning disable S2325, CA1822

namespace Backdash;

/// <inheritdoc cref="NetcodeSessionBuilder" />
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
public sealed class NetcodeSessionBuilder<TInput> where TInput : unmanaged
{
    readonly NetcodeSessionBuilder.SerializerFactory<TInput> serializer;
    readonly List<NetcodePlayer> playerList = [];

    SessionMode sessionMode = SessionMode.Remote;
    ServicesConfig<TInput>? sessionServices;

    NetcodeOptions options = new();
    SyncTestOptions<TInput>? syncTestOptions;
    SessionReplayOptions<TInput>? replayOptions;
    SpectatorOptions? spectatorOptions;

    internal NetcodeSessionBuilder(NetcodeSessionBuilder.SerializerFactory<TInput> serializer) =>
        this.serializer = serializer;

    /// <summary>
    ///     Builds a new <see cref="INetcodeSession{TInput}" />.
    /// </summary>
    public INetcodeSession<TInput> Build()
    {
        this.options.EnsureDefaults();
        var options = this.options.CloneOptions();

        SessionServices<TInput> services = new(
            serializer.Invoke(options.Protocol.NetworkSerializationEndianness), options, sessionServices
        );

        var session = GetSession();

        foreach (var player in playerList)
        {
            var addResult = session.AddPlayer(player);
            if (addResult is not ResultCode.Ok)
                throw new InvalidOperationException($"Failed to add player {playerList}: {addResult}");
        }

        return session;

        INetcodeSession<TInput> GetSession()
        {
            switch (sessionMode)
            {
                case SessionMode.Remote:
                    return new RemoteSession<TInput>(options, services);
                case SessionMode.Local:
                    EnsureLocalPlayers();
                    return new LocalSession<TInput>(options, services);
                case SessionMode.Spectator:
                    ConfigureSpectator();
                    return new SpectatorSession<TInput>(spectatorOptions, options, services);
                case SessionMode.Replay:
                    ConfigureReplay();
                    return new ReplaySession<TInput>(replayOptions, options, services);
                case SessionMode.SyncTest:
                    ConfigureSyncTest();
                    EnsureLocalPlayers();
                    return new SyncTestSession<TInput>(syncTestOptions, options, services);
                default:
                    throw new InvalidOperationException($"Unknown session mode: {sessionMode}");
            }
        }
    }

    void EnsureLocalPlayers()
    {
        if (playerList.Count > 0) return;

        for (var i = 0; i < options.NumberOfPlayers; i++)
            playerList.Add(new(PlayerType.Local));
    }

    /// <summary>
    ///     Set the <see cref="SessionMode" /> as <see cref="SessionMode.Remote" />.
    /// </summary>
    public NetcodeSessionBuilder<TInput> ForRemote() => WithMode(SessionMode.Remote);

    /// <summary>
    ///     Set the <see cref="SessionMode" /> as <see cref="SessionMode.Local" />.
    /// </summary>
    public NetcodeSessionBuilder<TInput> ForLocal() => WithMode(SessionMode.Local);

    /// <summary>
    ///     Set the <see cref="SessionMode" /> as <see cref="SessionMode.Spectator" />.
    /// </summary>
    public NetcodeSessionBuilder<TInput> ForSpectator(EndPoint hostEndpoint) =>
        ConfigureSpectator(options => options.HostEndPoint = hostEndpoint);

    /// <summary>
    ///     Set the <see cref="SessionMode" /> as <see cref="SessionMode.Spectator" />.
    /// </summary>
    public NetcodeSessionBuilder<TInput> ForSpectator(IPAddress hostAddress, int hostPort) =>
        ForSpectator(new IPEndPoint(hostAddress, hostPort));

    /// <summary>
    ///     Set the <see cref="SessionMode" /> as <see cref="SessionMode.Spectator" /> at localhost.
    /// </summary>
    public NetcodeSessionBuilder<TInput> ForSpectator(int hostPort) =>
        ForSpectator(new IPEndPoint(IPAddress.Loopback, hostPort));

    /// <summary>
    ///     Set the <see cref="SessionMode" /> as <see cref="SessionMode.Spectator" />.
    /// </summary>
    public NetcodeSessionBuilder<TInput> ForSpectator(Action<SpectatorOptions>? config = null) =>
        ConfigureSpectator(config);

    /// <summary>
    ///     Set the <see cref="SessionMode" /> as <see cref="SessionMode.Replay" />.
    /// </summary>
    public NetcodeSessionBuilder<TInput> ForReplay(Action<SessionReplayOptions<TInput>>? config = null) =>
        ConfigureReplay(config);

    /// <summary>
    ///     Set the <see cref="SessionMode" /> as <see cref="SessionMode.SyncTest" />.
    /// </summary>
    public NetcodeSessionBuilder<TInput> ForSyncTest(Action<SyncTestOptions<TInput>>? config = null) =>
        ConfigureSyncTest(config);

    /// <summary>
    ///     Set the <see cref="SessionMode" /> for the <see cref="INetcodeSession{TInput}" /> to be built.
    /// </summary>
    /// <value>Defaults to <see cref="SessionMode.Remote" /></value>
    public NetcodeSessionBuilder<TInput> WithMode(SessionMode mode)
    {
        if (!Enum.IsDefined(sessionMode))
            throw new ArgumentOutOfRangeException(nameof(mode), mode, null);

        sessionMode = mode;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.NumberOfPlayers"/>
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.NumberOfPlayers"/>
    public NetcodeSessionBuilder<TInput> WithPlayerCount(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        options.NumberOfPlayers = count;
        return this;
    }

    /// <summary>
    ///     Set the players for the <see cref="INetcodeSession{TInput}" />
    /// </summary>
    public NetcodeSessionBuilder<TInput> WithPlayers(params NetcodePlayer[] players)
    {
        ArgumentNullException.ThrowIfNull(players);
        playerList.AddRange(players);
        return this;
    }

    /// <summary>
    ///     Set the players for the <see cref="INetcodeSession{TInput}" />
    /// </summary>
    public NetcodeSessionBuilder<TInput> WithPlayers(IEnumerable<NetcodePlayer> players)
    {
        ArgumentNullException.ThrowIfNull(players);
        playerList.AddRange(players);
        return this;
    }

    /// <summary>
    ///     Set the session handler for the <see cref="INetcodeSession{TInput}" />
    /// </summary>
    /// <seealso cref="INetcodeSessionHandler" />
    /// <seealso cref="INetcodeSession.SetHandler" />
    public NetcodeSessionBuilder<TInput> WithHandler(INetcodeSessionHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return ConfigureServices(services => services.SessionHandler = handler);
    }

    /// <summary>
    ///     Configure <see cref="INetcodeSession{TInput}" /> options
    /// </summary>
    /// <seealso cref="NetcodeOptions" />
    public NetcodeSessionBuilder<TInput> Configure(Action<NetcodeOptions> config)
    {
        config.Invoke(options);
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.InputDelayFrames"/>
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.InputDelayFrames"/>
    public NetcodeSessionBuilder<TInput> WithInputDelayFrames(int frames)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frames);
        options.InputDelayFrames = frames;

        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.RollbackFramesSmoothFactor"/>
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.RollbackFramesSmoothFactor"/>
    public NetcodeSessionBuilder<TInput> WithRollbackCounterSmooth(float factor)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(factor);
        options.RollbackFramesSmoothFactor = factor;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.FrameRate"/>
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.FrameRate"/>
    public NetcodeSessionBuilder<TInput> WithFrameRate(int fps)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fps);
        options.FrameRate = fps;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.DeterministicRandomInitialSeed"/>
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.DeterministicRandomInitialSeed"/>
    public NetcodeSessionBuilder<TInput> WithInitialRandomSeed(uint value)
    {
        options.DeterministicRandomInitialSeed = value;
        return this;
    }

    /// <inheritdoc cref="WithInitialRandomSeed(uint)" />
    public NetcodeSessionBuilder<TInput> WithInitialRandomSeed(int value) =>
        WithInitialRandomSeed(unchecked((uint)value));

    /// <inheritdoc cref="NetcodeOptions.LocalPort"/>
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.LocalPort"/>
    public NetcodeSessionBuilder<TInput> WithPort(int port)
    {
        options.LocalPort = port;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.StateSizeHint" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.StateSizeHint"/>
    public NetcodeSessionBuilder<TInput> WithStateSizeHint(int byteSize)
    {
        options.StateSizeHint = int.Max(byteSize, 8);
        return this;
    }

    /// <inheritdoc cref="WithStateSizeHint(int)" />
    public NetcodeSessionBuilder<TInput> WithStateSizeHint(ByteSize size) =>
        WithStateSizeHint(size.Bytes);

    /// <inheritdoc cref="NetcodeOptions.SaveStateCount" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.SaveStateCount"/>
    public NetcodeSessionBuilder<TInput> WithSaveStateCount(int count)
    {
        options.SaveStateCount = count;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.DynamicFrameDelayPing" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.DynamicFrameDelayPing"/>
    public NetcodeSessionBuilder<TInput> WithDynamicFrameDelayPing(float milliseconds)
    {
        options.DynamicFrameDelayPing = milliseconds;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.MaxDynamicFrameDelay" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.MaxDynamicFrameDelay"/>
    public NetcodeSessionBuilder<TInput> WithMaxDynamicFrameDelay(int frames)
    {
        options.MaxDynamicFrameDelay = frames;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.UseBackgroundThreads" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.UseBackgroundThreads"/>
    public NetcodeSessionBuilder<TInput> WithBackgroundThreads(bool enabled = true)
    {
        options.UseBackgroundThreads = enabled;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.SaveConfirmedInputHistory" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.SaveConfirmedInputHistory"/>
    public NetcodeSessionBuilder<TInput> WithConfirmedInputHistory(bool enabled = true)
    {
        options.SaveConfirmedInputHistory = enabled;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.StateSerializationEndianness" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.StateSerializationEndianness"/>
    public NetcodeSessionBuilder<TInput> WithStateSerializationEndianness(Endianness? endianness)
    {
        options.StateSerializationEndianness = endianness;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.PredictionFrames" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.PredictionFrames"/>
    public NetcodeSessionBuilder<TInput> WithPredictionFrames(int amount)
    {
        options.PredictionFrames = amount;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.PredictionFramesOffset" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.PredictionFramesOffset"/>
    public NetcodeSessionBuilder<TInput> WithPredictionFramesOffset(int amount)
    {
        options.PredictionFramesOffset = amount;
        return this;
    }


    /// <inheritdoc cref="NetcodeOptions.InputQueueLength" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.InputQueueLength"/>
    public NetcodeSessionBuilder<TInput> WithInputQueueLength(int amount)
    {
        options.InputQueueLength = amount;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.UseIPv6" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.UseIPv6"/>
    public NetcodeSessionBuilder<TInput> WithIPv6(bool enabled)
    {
        options.UseIPv6 = enabled;
        return this;
    }

    /// <inheritdoc cref="NetcodeOptions.SynchronizationInterval" />
    /// <seealso cref="NetcodeOptions"/>
    /// <seealso cref="NetcodeOptions.SynchronizationInterval"/>
    public NetcodeSessionBuilder<TInput> WithSynchronizationInterval(int milliseconds)
    {
        options.SynchronizationInterval = milliseconds;
        return this;
    }

    /// <inheritdoc cref="WithSynchronizationInterval(int)" />
    public NetcodeSessionBuilder<TInput> WithSynchronizationInterval(TimeSpan interval) =>
        WithSynchronizationInterval((int)interval.TotalMilliseconds);

    /// <summary>
    ///     Sets <see cref="NetcodeOptions.Logger" /> log level
    /// </summary>
    /// <seealso cref="LogLevel" />
    public NetcodeSessionBuilder<TInput> WithLogLevel(LogLevel level, bool appendLevel = true) =>
        ConfigureLogger(o =>
        {
            o.EnabledLevel = level;
            o.AppendLevel = appendLevel;
        });

    /// <inheritdoc cref="ProtocolOptions.NetworkSerializationEndianness" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.NetworkSerializationEndianness"/>
    public NetcodeSessionBuilder<TInput> WithNetworkEndianness(Endianness endianness) =>
        ConfigureProtocol(p => p.NetworkSerializationEndianness = endianness);

    /// <inheritdoc cref="WithNetworkEndianness(Endianness)" />
    public NetcodeSessionBuilder<TInput> WithNetworkEndianness(bool useNetworkEndianness = true) =>
        WithNetworkEndianness(Platform.FindEndianness(useNetworkEndianness));

    /// <inheritdoc cref="ProtocolOptions.NetworkPackageStatsEnabled" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.NetworkPackageStatsEnabled"/>
    public NetcodeSessionBuilder<TInput> WithPackageStats(bool enabled = true) =>
        ConfigureProtocol(o => o.NetworkPackageStatsEnabled = enabled);

    /// <inheritdoc cref="ProtocolOptions.RemotePlayerStatsEnabled" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.RemotePlayerStatsEnabled"/>
    public NetcodeSessionBuilder<TInput> WithPlayerStats(bool enabled = true) =>
        ConfigureProtocol(o => o.RemotePlayerStatsEnabled = enabled);

    /// <inheritdoc cref="ProtocolOptions.QualityReportInterval" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.QualityReportInterval"/>
    public NetcodeSessionBuilder<TInput> WithQualityReportInterval(TimeSpan duration) =>
        ConfigureProtocol(o => o.QualityReportInterval = duration);

    /// <inheritdoc cref="WithQualityReportInterval(TimeSpan)" />
    public NetcodeSessionBuilder<TInput> WithQualityReportInterval(double milliseconds) =>
        WithQualityReportInterval(TimeSpan.FromMilliseconds(milliseconds));

    /// <inheritdoc cref="ProtocolOptions.ConsistencyCheckEnabled" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.ConsistencyCheckEnabled"/>
    public NetcodeSessionBuilder<TInput> WithConsistencyCheck(bool enabled) =>
        ConfigureProtocol(o => o.ConsistencyCheckEnabled = enabled);

    /// <inheritdoc cref="ProtocolOptions.ConsistencyCheckInterval" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.ConsistencyCheckInterval"/>
    public NetcodeSessionBuilder<TInput> WithConsistencyCheckInterval(TimeSpan duration) =>
        ConfigureProtocol(o => o.ConsistencyCheckInterval = duration);

    /// <inheritdoc cref="ProtocolOptions.DisconnectTimeoutEnabled" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.DisconnectTimeoutEnabled"/>
    public NetcodeSessionBuilder<TInput> WithDisconnectTimeout(bool enabled) =>
        ConfigureProtocol(o => o.DisconnectTimeoutEnabled = enabled);

    /// <inheritdoc cref="ProtocolOptions.DisconnectTimeout" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.DisconnectTimeout"/>
    public NetcodeSessionBuilder<TInput> WithDisconnectTimeout(TimeSpan duration, bool? enabled = null) =>
        ConfigureProtocol(o =>
        {
            o.DisconnectTimeout = duration;
            if (enabled.HasValue)
                o.DisconnectTimeoutEnabled = enabled.Value;
        });

    /// <inheritdoc cref="ProtocolOptions.DisconnectNotifyStart" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.DisconnectNotifyStart"/>
    public NetcodeSessionBuilder<TInput> WithDisconnectNotifyStart(TimeSpan duration) =>
        ConfigureProtocol(o => o.DisconnectNotifyStart = duration);

    /// <inheritdoc cref="ProtocolOptions.NetworkPackageStatsEnabled" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.NetworkPackageStatsEnabled"/>
    public NetcodeSessionBuilder<TInput> WithNetworkPackageStats(bool enabled = true) =>
        ConfigureProtocol(o => o.NetworkPackageStatsEnabled = enabled);

    /// <inheritdoc cref="ProtocolOptions.NumberOfSyncRoundTrips" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.NumberOfSyncRoundTrips"/>
    public NetcodeSessionBuilder<TInput> WithSyncRoundTrips(int count) =>
        ConfigureProtocol(o => o.NumberOfSyncRoundTrips = count);

    /// <inheritdoc cref="ProtocolOptions.NetworkLatency" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.NetworkLatency"/>
    public NetcodeSessionBuilder<TInput> WithNetworkLatency(TimeSpan latency, LatencyStrategy? strategy = null) =>
        ConfigureProtocol(o =>
        {
            o.NetworkLatency = latency;
            if (strategy.HasValue) o.LatencyStrategy = strategy.Value;
        });

    /// <inheritdoc cref="WithNetworkLatency(TimeSpan, LatencyStrategy?)"/>>
    public NetcodeSessionBuilder<TInput> WithNetworkLatency(long milliseconds, LatencyStrategy? strategy = null) =>
        WithNetworkLatency(TimeSpan.FromMilliseconds(milliseconds), strategy);

    /// <inheritdoc cref="ProtocolOptions.NetworkFixedLatency" />
    /// <seealso cref="ProtocolOptions"/>
    /// <seealso cref="ProtocolOptions.NetworkFixedLatency"/>
    public NetcodeSessionBuilder<TInput> WithNetworkFixedLatency(TimeSpan latency) =>
        ConfigureProtocol(o => o.NetworkFixedLatency = latency);

    /// <inheritdoc cref="WithNetworkFixedLatency(TimeSpan)"/>
    public NetcodeSessionBuilder<TInput> WithFixedNetworkLatency(long milliseconds) =>
        WithNetworkFixedLatency(TimeSpan.FromMilliseconds(milliseconds));

    /// <summary>
    ///     Set the logger <see cref="ServicesConfig{TInput}.LogWriter" />
    /// </summary>
    /// <seealso cref="NetcodeOptions.Logger" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithLogWriter(ILogWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        return ConfigureServices(s => s.LogWriter = writer);
    }

    /// <summary>
    ///     Set the logger <see cref="ServicesConfig{TInput}.LogWriter" />
    /// </summary>
    /// <seealso cref="NetcodeOptions.Logger" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithLogWriter<T>() where T : ILogWriter, new() =>
        WithLogWriter(new T());

    /// <summary>
    ///     Set the logger <see cref="ServicesConfig{TInput}.LogWriter" />
    /// </summary>
    /// <seealso cref="NetcodeOptions.Logger" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithLogWriter(Action<LogLevel, string> logAction) =>
        WithLogWriter(new DelegateLogWriter(logAction));

    /// <inheritdoc cref="WithLogWriter(ILogWriter)" />
    /// <seealso cref="FileTextLogWriter" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithFileLogWriter(string? filename = null, bool append = true) =>
        WithLogWriter(new FileTextLogWriter(filename, append));

    /// <summary>
    ///     Set the logger <see cref="ServicesConfig{TInput}.InputListener" />
    /// </summary>
    /// <seealso cref="IInputListener{TInput}" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithInputListener(IInputListener<TInput> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        return ConfigureServices(s => s.InputListener = listener);
    }

    /// <summary>
    ///     Set the logger <see cref="ServicesConfig{TInput}.InputListener" />
    /// </summary>
    /// <seealso cref="IInputListener{TInput}" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithInputListener<T>() where T : IInputListener<TInput>, new() =>
        WithInputListener(new T());

    /// <summary>
    ///     Set the <typeparamref name="TInput" /> comparer.
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithComparer(EqualityComparer<TInput> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        return ConfigureServices(s => s.InputComparer = comparer);
    }

    /// <summary>
    ///     Set the <typeparamref name="TInput" /> comparer.
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithComparer<T>() where T : EqualityComparer<TInput>, new() =>
        WithComparer(new T());

    /// <summary>
    ///     Set <see cref="INetcodeSession{TInput}" /> options
    /// </summary>
    /// <seealso cref="NetcodeOptions" />
    public NetcodeSessionBuilder<TInput> WithOptions(NetcodeOptions options)
    {
        this.options = options;
        return this;
    }

    /// <summary>
    ///     Set the <see cref="ServicesConfig{TInput}.ChecksumProvider" />
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithChecksumProvider(IChecksumProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        return ConfigureServices(s => s.ChecksumProvider = provider);
    }

    /// <summary>
    ///     Set the <see cref="ServicesConfig{TInput}.ChecksumProvider" />
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithChecksumProvider<T>() where T : IChecksumProvider, new() =>
        WithChecksumProvider(new T());

    /// <summary>
    ///     Set the <see cref="ServicesConfig{TInput}.ChecksumProvider" />
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithChecksumProvider(ChecksumDelegate compute) =>
        WithChecksumProvider(new DelegateChecksumProvider(compute));

    /// <summary>
    ///     Set the <see cref="ServicesConfig{TInput}.DeterministicRandom" />
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithDeterministicRandom(IDeterministicRandom<TInput> writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        return ConfigureServices(s => s.DeterministicRandom = writer);
    }

    /// <summary>
    ///     Set the <see cref="ServicesConfig{TInput}.DeterministicRandom" />
    /// </summary>
    /// <seealso cref="NetcodeOptions.Logger" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithDeterministicRandom<T>() where T : IDeterministicRandom<TInput>, new() =>
        WithDeterministicRandom(new T());

    /// <summary>
    ///     Set the <see cref="ServicesConfig{TInput}.StateStore" />
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithStateStore(IStateStore writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        return ConfigureServices(s => s.StateStore = writer);
    }

    /// <summary>
    ///     Set the <see cref="ServicesConfig{TInput}.StateStore" />
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithStateStore<T>() where T : IStateStore, new() =>
        WithStateStore(new T());

    /// <summary>
    ///     Set the <see cref="ServicesConfig{TInput}.PeerSocketFactory" />
    /// </summary>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithSocketFactory(IPeerSocketFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return ConfigureServices(s => s.PeerSocketFactory = factory);
    }

    /// <inheritdoc cref="WithSocketFactory(Backdash.Network.Client.IPeerSocketFactory)"/>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithSocketFactory<T>() where T : IPeerSocketFactory, new() =>
        WithSocketFactory(new T());

    /// <inheritdoc cref="WithSocketFactory(Backdash.Network.Client.IPeerSocketFactory)"/>
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithSocketFactory(Func<int, NetcodeOptions, IPeerSocket> factory) =>
        WithSocketFactory(new DelegateSocketFactory(factory));

    /// <summary>
    ///     Register a plugin by type
    /// </summary>
    /// <seealso cref="ServicesConfig{TInput}" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> UsePlugin(INetcodePlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        return ConfigureServices(services => services.Plugin = plugin);
    }

    /// <summary>
    ///     Use plugin type
    /// </summary>
    /// <seealso cref="ServicesConfig{TInput}" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> UsePlugin<TPlugin>() where TPlugin : INetcodePlugin, new() =>
        UsePlugin(new TPlugin());


    /// <summary>
    ///     Add a new custom job
    /// </summary>
    /// <seealso cref="ServicesConfig{TInput}" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> AddJob(INetcodeJob job)
    {
        ArgumentNullException.ThrowIfNull(job);
        return ConfigureServices(services => services.Jobs.Add(job));
    }

    /// <inheritdoc cref="AddJob(INetcodeJob)" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> AddJob<TPlugin>() where TPlugin : INetcodeJob, new() =>
        AddJob(new TPlugin());

    /// <summary>
    ///     Set custom session services
    /// </summary>
    /// <seealso cref="ServicesConfig{TInput}" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> WithServices(ServicesConfig<TInput> services)
    {
        ArgumentNullException.ThrowIfNull(services);
        sessionServices = services;
        return this;
    }

    /// <summary>
    ///     Configure custom session services
    /// </summary>
    /// <seealso cref="ServicesConfig{TInput}" />
    [MemberNotNull(nameof(sessionServices))]
    public NetcodeSessionBuilder<TInput> ConfigureServices(Action<ServicesConfig<TInput>> config)
    {
        var services = sessionServices ?? new();
        config.Invoke(services);
        return WithServices(services);
    }

    /// <summary>
    ///     Configure session logger
    /// </summary>
    /// <seealso cref="LoggerOptions" />
    public NetcodeSessionBuilder<TInput> ConfigureLogger(Action<LoggerOptions> config)
    {
        config.Invoke(options.Logger);
        return this;
    }

    /// <summary>
    ///     Configure session protocol
    /// </summary>
    /// <seealso cref="ProtocolOptions" />
    public NetcodeSessionBuilder<TInput> ConfigureProtocol(Action<ProtocolOptions> config)
    {
        config.Invoke(options.Protocol);
        return this;
    }

    /// <summary>
    ///     Configure session time synchronization
    /// </summary>
    /// <seealso cref="TimeSyncOptions" />
    public NetcodeSessionBuilder<TInput> ConfigureTimeSync(Action<TimeSyncOptions> config)
    {
        config.Invoke(options.TimeSync);
        return this;
    }

    /// <summary>
    ///     Set spectator session options.
    /// </summary>
    /// <seealso cref="SpectatorOptions" />
    public NetcodeSessionBuilder<TInput> WithSpectatorOptions(SpectatorOptions options)
    {
        spectatorOptions = options;
        return WithMode(SessionMode.Spectator);
    }

    /// <summary>
    ///     Configure spectator session options.
    /// </summary>
    /// <seealso cref="SpectatorOptions" />
    [MemberNotNull(nameof(spectatorOptions))]
    public NetcodeSessionBuilder<TInput> ConfigureSpectator(Action<SpectatorOptions>? config = null)
    {
        spectatorOptions ??= new();
        config?.Invoke(spectatorOptions);
        return WithMode(SessionMode.Spectator);
    }

    /// <summary>
    ///     Set sync test session options.
    /// </summary>
    /// <seealso cref="SyncTestOptions{TInput}" />
    public NetcodeSessionBuilder<TInput> WithSyncTestOptions(SyncTestOptions<TInput> options)
    {
        syncTestOptions = options;
        return WithMode(SessionMode.SyncTest);
    }

    /// <summary>
    ///     Configure sync test session options.
    /// </summary>
    /// <seealso cref="SyncTestOptions{TInput}" />
    [MemberNotNull(nameof(syncTestOptions))]
    public NetcodeSessionBuilder<TInput> ConfigureSyncTest(Action<SyncTestOptions<TInput>>? config = null)
    {
        syncTestOptions ??= new();
        config?.Invoke(syncTestOptions);
        return WithMode(SessionMode.SyncTest);
    }

    /// <summary>
    ///     Set replay session options.
    /// </summary>
    /// <seealso cref="SessionReplayOptions{TInput}" />
    public NetcodeSessionBuilder<TInput> WithReplayTestOptions(SessionReplayOptions<TInput> options)
    {
        replayOptions = options;
        return WithMode(SessionMode.Replay);
    }

    /// <summary>
    ///     Configure replay session options.
    /// </summary>
    /// <seealso cref="SessionReplayOptions{TInptut}" />
    [MemberNotNull(nameof(replayOptions))]
    public NetcodeSessionBuilder<TInput> ConfigureReplay(Action<SessionReplayOptions<TInput>>? config = null)
    {
        replayOptions ??= new();
        config?.Invoke(replayOptions);
        return WithMode(SessionMode.Replay);
    }
}

/// <summary>
///     Builder for <see cref="INetcodeSession{TInput}" />.
/// </summary>
/// <seealso cref="RollbackNetcode" />
[SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
public sealed class NetcodeSessionBuilder
{
    /// <summary>
    /// <see cref="IBinarySerializer{T}"/> factory function.
    /// </summary>
    internal delegate IBinarySerializer<T> SerializerFactory<T>(Endianness endianness) where T : unmanaged;

    /// <inheritdoc cref="InputTypeSelector" />
    public NetcodeSessionBuilder<T> WithInputType<T>(Func<InputTypeSelector, InputTypeSelected<T>> selector)
        where T : unmanaged => new(selector(new()).Serializer);

    /// <inheritdoc cref="InputTypeSelector.Enum{T}" />
    public NetcodeSessionBuilder<T> WithInputType<T>() where T : unmanaged, Enum =>
        WithInputType(x => x.Enum<T>());

    /// <summary>
    ///     Selected an input type for <see name="NetcodeSessionBuilder{TInput}" />
    /// </summary>
    public sealed class InputTypeSelected<T> where T : unmanaged
    {
        internal readonly SerializerFactory<T> Serializer;
        internal InputTypeSelected(SerializerFactory<T> serializer) => Serializer = serializer;
    }

    /// <summary>
    ///     Selector for <see cref="INetcodeSession{TInput}" /> input type
    /// </summary>
    [Serializable]
    public sealed class InputTypeSelector
    {
        internal InputTypeSelector() { }

        /// <summary>
        ///     Choose an <see cref="Enum" /> as <see cref="INetcodeSession{TInput}" /> input type
        /// </summary>
        public InputTypeSelected<T> Enum<T>() where T : unmanaged, Enum =>
            new(e => BinarySerializerFactory.ForEnum<T>(e));

        /// <summary>
        ///     Choose an <see cref="IBinaryInteger{T}" /> as <see cref="INetcodeSession{TInput}" /> input type
        /// </summary>
        public InputTypeSelected<T> Integer<T>() where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T> =>
            new(e => BinarySerializerFactory.ForInteger<T>(e));

        /// <summary>
        ///     Choose an <see cref="IBinaryInteger{T}" /> as <see cref="INetcodeSession{TInput}" /> input type
        /// </summary>
        public InputTypeSelected<T> Integer<T>(bool isUnsigned) where T : unmanaged, IBinaryInteger<T> =>
            new(e => IntegerBinarySerializer.Create<T>(isUnsigned, e));

        /// <summary>
        ///     Choose a raw unmanaged value type as an input type.
        ///     Must not be a reference type or a value type that contains references.
        ///     This *DOES NOT* use custom <see cref="Endianness" /> for <typeparamref name="T" /> integer fields.
        /// </summary>
        /// <seealso cref="RuntimeHelpers.IsReferenceOrContainsReferences{T}" />
        public InputTypeSelected<T> Struct<T>() where T : unmanaged =>
            new(_ => BinarySerializerFactory.ForStruct<T>());

        /// <summary>
        ///     Choose a custom type and serializer for the input type.
        /// </summary>
        public InputTypeSelected<T> Custom<T>(IBinarySerializer<T> serializer) where T : unmanaged
        {
            ArgumentNullException.ThrowIfNull(serializer);
            return new(_ => serializer);
        }

        /// <inheritdoc cref="Custom{T}(IBinarySerializer{T})" />
        public InputTypeSelected<T> Custom<T>(Func<Endianness, IBinarySerializer<T>> factory) where T : unmanaged
        {
            ArgumentNullException.ThrowIfNull(factory);
            return new(e => factory(e));
        }

        /// <inheritdoc cref="Custom{T}(IBinarySerializer{T})" />
        public InputTypeSelected<T> Custom<TSerializer, T>()
            where T : unmanaged where TSerializer : IBinarySerializer<T>, new() =>
            Custom(new TSerializer());
    }
}

#pragma warning restore S2325, CA1822
