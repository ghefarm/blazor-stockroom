using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class StockCheckWorkTests
{
    private TestDatabase database = null!;
    private StockCheckWork work = null!;

    // The eight sample products, and no time per product.
    [SetUp]
    public async Task CreateWork()
    {
        database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        work = new StockCheckWork(
            database.CreateFactory(), TimeSpan.Zero);
    }

    [TearDown]
    public async Task DisposeDatabase()
    {
        await database.DisposeAsync();
    }

    [Test]
    public void Run_ChecksEveryProductAndReportsEachOne()
    {
        List<StockCheckProgress> reports = [];
        var progress = new ListProgress(reports);

        var result = work.Run(progress, CancellationToken.None);

        Assert.That(result,
            Is.EqualTo(new StockCheckResult(8, 2, 2)));
        Assert.That(reports, Has.Count.EqualTo(8));
        Assert.That(reports[^1], Is.EqualTo(
            new StockCheckProgress(8, 8)));
    }

    [Test]
    public void Run_StopsAtOnceWhenCancelled()
    {
        List<StockCheckProgress> reports = [];
        var progress = new ListProgress(reports);

        Assert.That(
            () => work.Run(progress, new CancellationToken(true)),
            Throws.InstanceOf<OperationCanceledException>());
        Assert.That(reports, Is.Empty);
    }

    // Synchronous on purpose: Progress<T> would post the reports.
    private sealed class ListProgress(List<StockCheckProgress> list)
        : IProgress<StockCheckProgress>
    {
        public void Report(StockCheckProgress value) =>
            list.Add(value);
    }
}
