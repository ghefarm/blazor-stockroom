using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

// A fake that runs until the test calls Release or cancels it.
public sealed class BlockingWork : IStockCheckWork
{
    private readonly TaskCompletionSource release = new(
        TaskCreationOptions.RunContinuationsAsynchronously);
    private int runs;

    public int Runs => Volatile.Read(ref runs);

    public void Release() => release.TrySetResult();

    public StockCheckResult Run(
        IProgress<StockCheckProgress> progress,
        CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref runs);
        release.Task.Wait(cancellationToken);
        progress.Report(new(1, 1));
        return new StockCheckResult(1, 0, 0);
    }
}
