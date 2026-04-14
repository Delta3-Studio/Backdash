using Backdash.Core;
using Backdash.Network;
using Backdash.Network.Client;
using Backdash.Network.Protocol;
using Backdash.Options;
using Backdash.Serialization;
using Backdash.Synchronizing.Input.Confirmed;
using Backdash.Synchronizing.Random;
using Backdash.Synchronizing.State;

namespace Backdash;

sealed class SessionServices<TInput> where TInput : unmanaged
{
    public IBinarySerializer<TInput> InputSerializer { get; }
    public IChecksumProvider ChecksumProvider { get; }
    public Logger Logger { get; }
    public NetcodeJobManager JobManager { get; }
    public ProtocolClientFactory ProtocolClientFactory { get; }
    public IStateStore StateStore { get; }
    public ChecksumStore ChecksumStore { get; }
    public IRandomNumberGenerator Random { get; }
    public IDeterministicRandom<TInput> DeterministicRandom { get; }
    public ILatencyStrategy LatencyStrategy { get; }
    public IInputListener<TInput>? InputListener { get; }

    public EqualityComparer<TInput> InputComparer { get; }

    public INetcodeSessionHandler SessionHandler { get; }

    public PluginManager PluginManager { get; }
    public INetcodeJob[] Jobs { get; set; }

    public SessionServices(
        IBinarySerializer<TInput> inputSerializer,
        NetcodeOptions options,
        ServicesConfig<TInput>? services
    )
    {
        ArgumentNullException.ThrowIfNull(inputSerializer);
        ArgumentNullException.ThrowIfNull(options);

        ChecksumProvider = services?.ChecksumProvider ?? new Fletcher32ChecksumProvider();
        StateStore = services?.StateStore ?? new DefaultStateStore(options.StateSizeHint);
        InputListener = services?.InputListener;
        Random = new DefaultRandomNumberGenerator(services?.Random ?? System.Random.Shared);
        LatencyStrategy = DelayStrategyFactory.Create(Random, options.Protocol.LatencyStrategy);
        InputComparer = services?.InputComparer ?? EqualityComparer<TInput>.Default;
        InputSerializer = inputSerializer;
        DeterministicRandom = services?.DeterministicRandom ?? new XorShiftRandom<TInput>();
        if (DeterministicRandom.InitialSeed is 0)
            DeterministicRandom.SetInitialSeed(options.DeterministicRandomInitialSeed);

        ChecksumStore = new(Math.Max(options.TotalSavedFramesAllowed, options.Protocol.ConsistencyCheckStoreSize)
                            + options.Protocol.ConsistencyCheckDistance);

        var logWriter = services?.LogWriter ?? new ConsoleTextLogWriter();
        Logger = new(options.Logger, logWriter);
        JobManager = new(Logger);
        SessionHandler = services?.SessionHandler ?? new EmptySessionHandler(Logger);

        var socketFactory = services?.PeerSocketFactory ?? new PeerSocketFactory();
        ProtocolClientFactory = new(options, socketFactory, Logger, LatencyStrategy);
        Jobs = services?.Jobs.ToArray() ?? [];
        PluginManager = new(Logger, services?.Plugin);
    }
}
