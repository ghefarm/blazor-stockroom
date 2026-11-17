using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class StockCheckCoordinatorTests
{
    // NUnit shares one fixture instance between its tests, so a
    // subscriber from one test must not reach the next.
    private StockCheckStatus status = null!;

    [SetUp]
    public void CreateStatus() =>
        status = new(NullLogger<StockCheckStatus>.Instance);

    [Test]
    public async Task TryStart_AllowsOnlyOneRunAtATime()
    {
        var work = new BlockingWork();
        var coordinator = CreateCoordinator(work);

        var started = await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => Task.Run(coordinator.TryStart)));
        var busy = status.Current.IsRunning;
        work.Release();
        await Poll.UntilAsync(() => !status.Current.IsRunning);

        Assert.That(started.Count(s => s), Is.EqualTo(1));
        Assert.That(work.Runs, Is.EqualTo(1));
        Assert.That(busy, Is.True);
        Assert.That(status.Current.ResultText,
            Is.EqualTo("Checked 1 product: 0 low, 0 out of stock."));
    }

    [Test]
    public async Task Coordinator_IsFreeAgainAfterEveryKindOfEnd()
    {
        // The regression: a subscriber whose circuit is gone
        // throws. The run must still end and free the gate.
        status.Changed += _ =>
            throw new ObjectDisposedException("circuit");
        var finished = new BlockingWork();
        finished.Release();
        var coordinator = CreateCoordinator(finished);

        Assert.That(coordinator.TryStart(), Is.True);
        await Poll.UntilAsync(coordinator.TryStart);
        await Poll.UntilAsync(() => finished.Runs == 2
            && !status.Current.IsRunning);

        // A cancel is reported as a cancel, not as an error.
        var blocked = new BlockingWork();
        var second = CreateCoordinator(blocked);
        second.TryStart();
        second.Cancel();
        await Poll.UntilAsync(() => !status.Current.IsRunning);
        Assert.That(status.Current.WasCancelled, Is.True);

        // A failure is logged and reported, and frees the gate.
        var failing = Substitute.For<IStockCheckWork>();
        failing.Run(default!, default)
            .ThrowsForAnyArgs(new IOException("disk"));
        var third = CreateCoordinator(failing);
        third.TryStart();
        await Poll.UntilAsync(() => !status.Current.IsRunning);
        Assert.That(status.Current.ResultText,
            Is.EqualTo("The stock check failed. See the log."));
        await Poll.UntilAsync(third.TryStart);
    }

    private StockCheckCoordinator CreateCoordinator(
        IStockCheckWork work) =>
        new(status, work, CancellationToken.None,
            NullLogger<StockCheckCoordinator>.Instance);
}
