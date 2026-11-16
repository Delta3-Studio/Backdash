using System.Diagnostics;
using Backdash.Core;

namespace Backdash.Network;

/// <summary>
///     Jitter latency delay strategy
/// </summary>
public enum LatencyStrategy
{
    /// <summary>Constant delay</summary>
    Constant,

    /// <summary>Random gaussian delay</summary>
    Gaussian,

    /// <summary>Random continuous delay</summary>
    ContinuousUniform,
}

interface ILatencyStrategy
{
    double Jitter(double latencyMs);
}

static class DelayStrategyFactory
{
    public static ILatencyStrategy Create(IRandomNumberGenerator random, LatencyStrategy strategy) => strategy switch
    {
        LatencyStrategy.Constant => new ConstantLatencyStrategy(),
        LatencyStrategy.Gaussian => new GaussianLatencyStrategy(random),
        LatencyStrategy.ContinuousUniform => new UniformLatencyStrategy(random),
        _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null),
    };
}

sealed class ConstantLatencyStrategy : ILatencyStrategy
{
    public double Jitter(double latencyMs) => latencyMs;
}

sealed class UniformLatencyStrategy(IRandomNumberGenerator random) : ILatencyStrategy
{
    public double Jitter(double latencyMs)
    {
        var mean = latencyMs * 2.0 / 3.0;
        return (latencyMs / 3.0) + (random.NextDouble() * mean);
    }
}

sealed class GaussianLatencyStrategy(IRandomNumberGenerator random) : ILatencyStrategy
{
    public double Jitter(double latencyMs)
    {
        var mean = latencyMs / 2.0;
        var sigma = (latencyMs - mean) / 3.0;
        var std = random.NextGaussian();
        return Math.Clamp((std * sigma) + mean, 0.0, latencyMs);
    }
}

sealed class LatencyWaiter
{
    readonly double jitterRange;
    readonly double baseLatency;
    readonly ILatencyStrategy strategy;

    LatencyWaiter(ILatencyStrategy strategy, TimeSpan jitterRange, TimeSpan baseLatency)
    {
        this.strategy = strategy;
        this.jitterRange = jitterRange.TotalMilliseconds;
        this.baseLatency = baseLatency.TotalMilliseconds;
    }

    public void Wait(long timestamp)
    {
        var delayMs = baseLatency + strategy.Jitter(jitterRange);
        var delayTicks = (long)(delayMs * (Stopwatch.Frequency / 1000.0));
        var deadline = timestamp + delayTicks;

        // LATER: validate usage of Task.Delay
        SpinWait sw = new();
        while (Stopwatch.GetTimestamp() < deadline)
            sw.SpinOnce();
    }

    public static LatencyWaiter? Create(
        ILatencyStrategy strategy,
        TimeSpan jitterRange,
        TimeSpan? baseLatency = null
    )
    {
        var fixLatency = baseLatency ?? TimeSpan.Zero;
        if (fixLatency + jitterRange <= TimeSpan.Zero) return null;
        return new(strategy, jitterRange, fixLatency);
    }
}
