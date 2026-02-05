namespace SpaceWar.Services;

public static class AsyncTimer
{
    public static async Task Create(
        TimeSpan interval,
        Func<Task> main,
        Action<Exception>? errorHandler = null,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(main);
        using PeriodicTimer timer = new(interval);

        try
        {
            while (await timer.WaitForNextTickAsync(ct))
                await main.Invoke();
        }
        catch (OperationCanceledException)
        {
            // skip
        }
        catch (Exception ex)
        {
            errorHandler?.Invoke(ex);
        }
    }
}
