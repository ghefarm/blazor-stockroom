using Microsoft.Extensions.Logging.Abstractions;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class StockCheckStatusTests
{
    // A new status per test: NUnit shares the fixture instance.
    private StockCheckStatus status = null!;

    [SetUp]
    public void CreateStatus() =>
        status = new(NullLogger<StockCheckStatus>.Instance);

    [Test]
    public void Progress_PublishesEachWholePercentOnce()
    {
        List<int> published = [];
        status.Changed += state => published.Add(state.Percent);
        var progress = new PercentProgress(status);

        for (var done = 1; done <= 1000; done++)
        {
            progress.Report(new(done, 1000));
        }

        Assert.That(published, Has.Count.EqualTo(100));
        Assert.That(published[^1], Is.EqualTo(100));
    }

    [Test]
    public void Update_ReachesEverySubscriberWhenOneThrows()
    {
        var reached = 0;
        status.Changed += _ => throw new ObjectDisposedException("x");
        status.Changed += _ => reached++;

        status.Update(_ => StockCheckState.Started);

        Assert.That(reached, Is.EqualTo(1));
        Assert.That(status.Current.IsRunning, Is.True);
    }
}
