using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Backdash.Core;

sealed class NetcodeJobManager(Logger logger) : IDisposable
{
    readonly HashSet<JobEntry> jobs = [];
    readonly CancellationTokenSource cts = new();
    readonly Lock locker = new();
    readonly ConcurrentQueue<Exception> exceptions = [];

    readonly Channel<JobEntry> completedJobs = Channel.CreateUnbounded<JobEntry>(new()
    {
        SingleReader = true,
        SingleWriter = false,
    });

    bool enabled;
    bool disposed;
    Task? startTask;
    CancellationToken StoppingToken => cts.Token;

    public bool IsRunning
    {
        get
        {
            lock (locker)
                return enabled;
        }
    }

    public Task Start(bool onThread, CancellationToken cancellationToken)
    {
        lock (locker)
        {
            if (startTask is not null) return startTask;
            enabled = true;
            startTask = onThread ? Task.Run(StartJobs, CancellationToken.None) : StartJobs();
            return startTask;
        }

        async Task StartJobs()
        {
            try
            {
                logger.Write(LogLevel.Debug, "Starting background tasks");
                await using var _ = cancellationToken.Register(() => Stop(TimeSpan.Zero));

                lock (locker)
                {
                    if (StoppingToken.IsCancellationRequested)
                    {
                        completedJobs.Writer.TryComplete();
                        return;
                    }

                    foreach (var job in jobs.ToArray())
                        StartJobTask(job);
                }

                await WaitJobsToFinish();
                logger.Write(LogLevel.Debug, "Finished background tasks");
            }
            finally
            {
                lock (locker)
                    enabled = false;
            }
        }

        async Task WaitJobsToFinish()
        {
            while (true)
            {
                lock (locker)
                {
                    if (jobs.Count is 0)
                    {
                        enabled = false;
                        completedJobs.Writer.TryComplete();
                        return;
                    }
                }

                var completedJob = await completedJobs.Reader.ReadAsync(CancellationToken.None).ConfigureAwait(false);
                lock (locker)
                {
                    if (!jobs.Remove(completedJob))
                        continue;
                }

                logger.Write(LogLevel.Debug, $"Completed: {completedJob.Name}");
            }
        }
    }

    void StartJobTask(JobEntry entry) => _ = RunJobTask(entry);

    async Task RunJobTask(JobEntry entry)
    {
        var job = entry.Job;
        try
        {
            logger.Write(LogLevel.Trace, $"job {job.JobName}: start");
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(StoppingToken, entry.StoppingToken);
            try
            {
                await job.Start(linkedCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                logger.Write(LogLevel.Debug, $"job {job.JobName} stopped");
            }
            catch (ChannelClosedException)
            {
                logger.Write(LogLevel.Debug, $"job {job.JobName} channel closed");
            }
            catch (NetcodeAssertionException ex) when (StoppingToken.IsCancellationRequested)
            {
                logger.Write(LogLevel.Debug, $"job {job.JobName} skip assert: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.Write(LogLevel.Error, $"job {job.JobName} error: {ex}");
                cts.CancelAfter(TimeSpan.FromMilliseconds(100));
                exceptions.Enqueue(ex);
            }
        }
        finally
        {
            await FinishJob();
        }

        return;

        async Task FinishJob()
        {
            try
            {
                switch (entry.Job)
                {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Write(LogLevel.Error, $"job dispose {job.JobName} error: {ex}");
                exceptions.Enqueue(ex);
            }
            finally
            {
                completedJobs.Writer.TryWrite(entry);
            }
        }
    }

    public void ThrowIfError()
    {
        List<Exception> errors = [];
        while (exceptions.TryDequeue(out var error))
            errors.Add(error);

        if (errors.Count is not 0)
            throw new AggregateException(errors);
    }

    public void Register(INetcodeJob job, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(job);
        lock (locker)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            JobEntry entry = new(job, cancellationToken);
            if (!jobs.Add(entry)) return;
            if (IsRunning) StartJobTask(entry);
        }
    }

    public void Stop(TimeSpan timeout = default)
    {
        lock (locker)
        {
            if (disposed || !enabled) return;
        }

        if (cts.IsCancellationRequested) return;

        if (timeout <= TimeSpan.Zero)
            cts.Cancel();
        else
            cts.CancelAfter(timeout);
    }

    public void Dispose()
    {
        Task? pending;
        bool shouldStop;
        lock (locker)
        {
            if (disposed) return;
            disposed = true;
            shouldStop = enabled;
            pending = startTask;
        }

        if (shouldStop && !cts.IsCancellationRequested)
            cts.Cancel();

        if (pending is null || pending.IsCompleted)
            cts.Dispose();
        else
        {
            _ = pending.ContinueWith(
                _ => cts.Dispose(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
        }
    }

    sealed class JobEntry(INetcodeJob job, CancellationToken stoppingToken) : IEquatable<JobEntry>
    {
        public INetcodeJob Job { get; } = job;
        public CancellationToken StoppingToken { get; } = stoppingToken;
        public string Name { get; } = job.JobName ?? job.GetType().Name;
        public bool Equals(JobEntry? other) => other is not null && Name.Equals(other.Name);
        public override bool Equals(object? obj) => obj is JobEntry other && Equals(other);
        public override int GetHashCode() => Name.GetHashCode();
        public override string ToString() => $"Job<{Name}>";
    }
}
