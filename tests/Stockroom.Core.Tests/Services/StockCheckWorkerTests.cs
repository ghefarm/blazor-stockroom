using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class StockCheckWorkerTests
{
    [Test]
    public async Task Worker_RunsQueuedJobsOneAfterAnother()
    {
        var first = new BlockingWork();
        var second = new TrackedWork();
        await using var provider = CreateProvider(first, second);
        var worker = provider.GetRequiredService<StockCheckWorker>();
        await worker.StartAsync(CancellationToken.None);

        Assert.That(worker.TryStart(), Is.True);
        await Poll.UntilAsync(() => first.Runs == 1);
        var waiting = worker.TryStart();
        var full = !worker.TryStart();
        var overlapped = second.Runs > 0;
        first.Release();
        await Poll.UntilAsync(() => second.IsDisposed);

        Assert.That(waiting, Is.True);
        Assert.That(full, Is.True);
        Assert.That(overlapped, Is.False);
        Assert.That(second.Runs, Is.EqualTo(1));
        await worker.StopAsync(CancellationToken.None);
    }

    [Test]
    public async Task Worker_SurvivesFailureCancelAndShutdown()
    {
        var failing = Substitute.For<IStockCheckWork>();
        failing.Run(default!, default)
            .ThrowsForAnyArgs(new IOException("disk"));
        var cancelled = new BlockingWork();
        var stopped = new BlockingWork();
        await using var provider =
            CreateProvider(failing, cancelled, stopped);
        var worker = provider.GetRequiredService<StockCheckWorker>();
        var status = provider.GetRequiredService<StockCheckStatus>();
        await worker.StartAsync(CancellationToken.None);

        worker.TryStart();
        await Poll.UntilAsync(() => status.Current.ResultText
            == "The stock check failed. See the log.");
        worker.TryStart();
        await Poll.UntilAsync(() => cancelled.Runs == 1);
        worker.Cancel();
        await Poll.UntilAsync(() => status.Current.WasCancelled);

        // StopAsync returns only after the running job has ended.
        worker.TryStart();
        await Poll.UntilAsync(() => stopped.Runs == 1);
        await worker.StopAsync(CancellationToken.None);
        Assert.That(status.Current.IsRunning, Is.False);
        Assert.That(status.Current.WasCancelled, Is.True);
    }

    // Each job resolves its work from a new scope, so the fakes
    // are handed out in order, one per job.
    private static ServiceProvider CreateProvider(
        params IStockCheckWork[] works)
    {
        var queue = new Queue<IStockCheckWork>(works);
        var services = new ServiceCollection();
        services.AddSingleton(
            typeof(ILogger<>), typeof(NullLogger<>));
        services.AddStockCheckWorker();
        services.AddScoped(_ => queue.Dequeue());
        return services.BuildServiceProvider(validateScopes: true);
    }

    private sealed class TrackedWork : IStockCheckWork, IDisposable
    {
        private int runs;
        private int disposed;

        public int Runs => Volatile.Read(ref runs);

        public bool IsDisposed => Volatile.Read(ref disposed) == 1;

        public StockCheckResult Run(
            IProgress<StockCheckProgress> progress,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref runs);
            return new StockCheckResult(1, 0, 0);
        }

        public void Dispose() =>
            Interlocked.Exchange(ref disposed, 1);
    }
}
