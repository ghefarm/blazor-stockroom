using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class ProductSummaryServiceTests
{
    private const int PalletJack = 2;

    private TestDatabase database = null!;
    private ServiceProvider provider = null!;

    [SetUp]
    public async Task CreateProvider()
    {
        database = await TestDatabase.CreateAsync();
        await database.SeedAsync();

        var services = new ServiceCollection();
        services.AddSingleton(database.CreateFactory());
        services.AddHybridCache();
        services.AddScoped<ProductSummaryService>();
        services.AddScoped<ProductCatalog>();
        var validating = new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        };
        provider = services.BuildServiceProvider(validating);
    }

    [TearDown]
    public async Task DisposeProvider()
    {
        await provider.DisposeAsync();
        await database.DisposeAsync();
    }

    [Test]
    public async Task Service_ServesFreshValueAfterEveryWrite()
    {
        // Two scopes, like two circuits: one reads, one writes.
        using var reading = provider.CreateScope();
        using var writing = provider.CreateScope();
        var summaries = reading.ServiceProvider
            .GetRequiredService<ProductSummaryService>();
        var catalog = writing.ServiceProvider
            .GetRequiredService<ProductCatalog>();

        var before = await summaries.SummarizeAsync(StockLevel.Low);
        await catalog.ChangeStockQuantityAsync(PalletJack, 13);
        var afterCount =
            await summaries.SummarizeAsync(StockLevel.Low);
        // Version 1: the new quantity added 1 (chapter 24).
        await catalog.ChangePriceAsync(PalletJack, 36900, 1);
        var afterPrice = await summaries.SummarizeAsync(null);

        Assert.That(before.Count, Is.EqualTo(2));
        Assert.That(afterCount.Count, Is.EqualTo(1));
        Assert.That(afterPrice.StockValueCents, Is.EqualTo(731820));
    }

    [Test]
    public async Task HybridCache_KeepsValueFromFactoryStartedBefore()
    {
        var cache = provider.GetRequiredService<HybridCache>();
        var version = 1;
        var computed = new TaskCompletionSource();
        var release = new TaskCompletionSource();

        var slowRead = cache.GetOrCreateAsync("summary", async _ =>
        {
            var seen = version;
            computed.SetResult();
            await release.Task;
            return seen;
        }, tags: ["product-summary"]).AsTask();

        await computed.Task;
        version = 2;
        await cache.RemoveByTagAsync("product-summary");
        release.SetResult();
        await slowRead;

        var next = await cache.GetOrCreateAsync("summary",
            _ => ValueTask.FromResult(version),
            tags: ["product-summary"]);
        Assert.That(next, Is.EqualTo(1), "stale until it expires");
    }
}
