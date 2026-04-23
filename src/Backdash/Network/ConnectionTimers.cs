using Backdash.Options;
using Timer = System.Timers.Timer;

namespace Backdash.Network;

sealed class ConnectionTimers(ProtocolOptions options) : IDisposable
{
    public readonly Timer QualityReport = new(options.QualityReportInterval);
    public readonly Timer NetworkStats = new(options.NetworkPackageStatsInterval);
    public readonly Timer KeepAlive = new(options.KeepAliveInterval);
    public readonly Timer ResendInputs = new(options.ResendInputInterval);
    public readonly Timer ConsistencyCheck = new(options.ConsistencyCheckInterval);

    public void Dispose()
    {
        Stop();

        KeepAlive.Dispose();
        ResendInputs.Dispose();
        QualityReport.Dispose();
        NetworkStats.Dispose();
        ConsistencyCheck.Dispose();
    }

    public void Start()
    {
        KeepAlive.Start();
        QualityReport.Start();
        ResendInputs.Start();

        if (options.IsNetworkPackageStatsEnabled())
            NetworkStats.Start();

        if (options.IsConsistencyCheckEnabled())
            ConsistencyCheck.Start();
    }

    public void Stop()
    {
        KeepAlive.Stop();
        QualityReport.Stop();
        ResendInputs.Stop();

        if (options.IsNetworkPackageStatsEnabled())
            NetworkStats.Stop();

        if (options.IsConsistencyCheckEnabled())
            ConsistencyCheck.Stop();
    }
}
