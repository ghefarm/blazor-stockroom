using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Stockroom.Core.ReadOnly;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.ReadOnly;

public class ReadOnlyRuleTests
{
    private static readonly DateOnly Ended = new(2026, 10, 31);
    private static readonly DateOnly Paid = new(2027, 10, 31);

    [Test]
    public async Task WritesWhilePaidAndAlwaysTakesASubscription()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var state = new SubscriptionState(CreateClock());
        var factory = database.CreateFactoryWith(
            new ReadOnlyInterceptor(state));
        var catalog = new ProductCatalog(
            factory, TestDatabase.CreateSummaries(factory));
        var subscriptions = new SubscriptionService(factory, state);

        // Read-only first: accounts and the subscription are free.
        state.Set(Ended);
        var before = state.AllowsWrites;
        await using (var users = await factory.CreateDbContextAsync())
        {
            users.Users.Add(new() { UserName = "nora" });
            await users.SaveChangesAsync();
        }

        await subscriptions.ExtendAsync(Paid);
        var saved = await catalog.ChangePriceAsync(1, 9500, 0);
        await using var db = await factory.CreateDbContextAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(before, Is.False);
            Assert.That(state.AllowsWrites, Is.True);
            Assert.That(saved, Is.EqualTo(SaveOutcome.Saved));
            Assert.That(db.HasReadOnlyRule(), Is.True);
        }
    }

    [Test]
    public async Task RefusesBusinessWritesWhileReadOnly()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var clock = CreateClock();
        var state = new SubscriptionState(clock);
        var notLoaded = state.AllowsWrites;
        state.Set(Ended);
        var factory = database.CreateFactoryWith(
            new ReadOnlyInterceptor(state));
        var catalog = new ProductCatalog(
            factory, TestDatabase.CreateSummaries(factory));

        Assert.ThrowsAsync<ReadOnlyModeException>(
            () => catalog.ChangePriceAsync(1, 9500, 0));
        Assert.ThrowsAsync<ReadOnlyModeException>(
            () => catalog.DeactivateAsync(["GL-M"]));

        // The import asks before it writes its first file.
        var folder = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            Guid.NewGuid().ToString("N"));
        var importer = new PriceListImporter(
            factory, new ImportArchive(folder), clock,
            TestDatabase.CreateSummaries(factory));
        var import = await importer.ImportAsync("list.csv",
            new MemoryStream("SKU;Price\nTP-050;2,59\n"u8.ToArray()));

        // The hole that ThrowIfReadOnly closes: ExecuteUpdate
        // never meets the interceptor.
        await using var db = await factory.CreateDbContextAsync();
        var bypassed = await db.Products
            .Where(p => p.Sku == "TP-050")
            .ExecuteUpdateAsync(s => s.SetProperty(
                p => p.StockQuantity, 0));

        await using var plain = database.CreateContext();
        var shelf = await plain.Products.SingleAsync(p => p.Id == 1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(notLoaded, Is.False);
            Assert.That(import.Errors, Is.EqualTo(new[]
            {
                new ReadOnlyModeException().Message,
            }));
            Assert.That(Directory.Exists(folder), Is.False);
            Assert.That(bypassed, Is.EqualTo(1));
            Assert.That(shelf.PriceCents, Is.EqualTo(8900));
            Assert.That(plain.HasReadOnlyRule(), Is.False);
        }
    }

    private static BusinessClock CreateClock()
    {
        var time = Substitute.For<TimeProvider>();
        time.GetUtcNow().Returns(
            new DateTimeOffset(
                2026, 11, 17, 12, 0, 0, TimeSpan.Zero));
        return new BusinessClock(time,
            TimeZoneInfo.FindSystemTimeZoneById("America/Chicago"));
    }
}
