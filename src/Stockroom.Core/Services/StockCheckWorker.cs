using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Stockroom.Core.Services;

public sealed record StockCheckJob(Guid Id);

public sealed class StockCheckWorker(
    StockCheckStatus status,
    IServiceScopeFactory scopeFactory,
    ILogger<StockCheckWorker> logger)
    : BackgroundService, IStockCheckRunner
{
    // Room for one waiting job. A second waiting job would only
    // repeat the same check.
    private readonly Channel<StockCheckJob> queue =
        Channel.CreateBounded<StockCheckJob>(
            new BoundedChannelOptions(1) { SingleReader = true });

    private readonly Lock cancelLock = new();
    private CancellationTokenSource? current;

    public bool TryStart()
    {
        var job = new StockCheckJob(Guid.NewGuid());
        return queue.Writer.TryWrite(job);
    }

    public void Cancel()
    {
        lock (cancelLock)
        {
            current?.Cancel();
        }
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        // One reader takes one job at a time, so two runs never
        // overlap, and no gate is needed.
        var jobs = queue.Reader.ReadAllAsync(stoppingToken);
        await foreach (var job in jobs)
        {
            await RunJobAsync(job, stoppingToken);
        }
    }

    private async Task RunJobAsync(
        StockCheckJob job, CancellationToken stoppingToken)
    {
        using var source = CancellationTokenSource
            .CreateLinkedTokenSource(stoppingToken);
        lock (cancelLock)
        {
            current = source;
        }

        // A scope of its own: scoped services live exactly as long
        // as this job, and no circuit can dispose them.
        await using var scope = scopeFactory.CreateAsyncScope();
        var work = scope.ServiceProvider
            .GetRequiredService<IStockCheckWork>();

        status.Update(_ => StockCheckState.Started);
        logger.LogInformation("Stock check {JobId} started.", job.Id);
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
            // Caught here: an exception that leaves ExecuteAsync
            // stops the whole application by default.
            logger.LogError(
                ex, "Stock check {JobId} failed.", job.Id);
            status.Update(state => state.MarkFailed());
        }
        finally
        {
            lock (cancelLock)
            {
                current = null;
            }
        }
    }
}
