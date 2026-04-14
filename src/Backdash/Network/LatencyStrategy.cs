using Backdash.Core;

namespace Backdash.Network;

/// <summary>
///     Jitter delay strategy
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
    TimeSpan Jitter(TimeSpan sendLatency);
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
    public TimeSpan Jitter(TimeSpan sendLatency) => sendLatency;
}

sealed class UniformLatencyStrategy(IRandomNumberGenerator random) : ILatencyStrategy
{
    public TimeSpan Jitter(TimeSpan sendLatency)
    {
        var latency = sendLatency.TotalMilliseconds;
        var mean = latency * 2 / 3;
        var ms = mean + (random.NextInt() % latency / 3);
        return TimeSpan.FromMilliseconds(ms);
    }
}

sealed class GaussianLatencyStrategy(IRandomNumberGenerator random) : ILatencyStrategy
{
    public TimeSpan Jitter(TimeSpan sendLatency)
    {
        var latency = sendLatency.TotalMilliseconds;
        var mean = latency / 2;
        var sigma = (latency - mean) / 3;
        var std = random.NextGaussian();
        var ms = (int)Math.Clamp((std * sigma) + mean, 0, latency);
        return TimeSpan.FromMilliseconds(ms);
    }
}
