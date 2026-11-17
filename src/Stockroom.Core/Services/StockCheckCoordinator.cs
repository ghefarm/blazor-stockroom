using Microsoft.Extensions.Logging;

namespace Stockroom.Core.Services;

public sealed class StockCheckCoordinator(
    StockCheckStatus status,
    IStockCheckWork work,
    CancellationToken applicationStopping,
    ILogger<StockCheckCoordinator> logger) : IStockCheckRunner
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly Lock cancelLock = new();
    private CancellationTokenSource? cancellation;

    public bool TryStart()
    {
        // Wait(0) never waits: it takes the gate or returns false.
        if (!gate.Wait(0))
        {
            return false;
        }

        var source = CancellationTokenSource
            .CreateLinkedTokenSource(applicationStopping);
        lock (cancelLock)
        {
            cancellation = source;
        }

        status.Update(_ => StockCheckState.Started);
        _ = Task.Run(() => Run(source));
        return true;
    }

    public void Cancel()
    {
        // Inside the lock, so Cancel never meets a source that the
        // end of the run has already disposed.
        lock (cancelLock)
        {
            cancellation?.Cancel();
        }
    }

    private void Run(CancellationTokenSource source)
    {
        try
        {
            var result = work.Run(
                new PercentProgress(status), source.Token);
            status.Update(state => state.Finish(result));
        }
        catch (OperationCanceledException)
            when (source.IsCancellationRequested)
        {
            status.Update(state => state.MarkCancelled());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The stock check failed.");
            status.Update(state => state.MarkFailed());
        }
        finally
        {
            lock (cancelLock)
            {
                cancellation = null;
            }

            source.Dispose();
            gate.Release();
        }
    }
}
