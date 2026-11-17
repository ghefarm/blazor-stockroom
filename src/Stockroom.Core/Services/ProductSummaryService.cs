using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

public sealed class ProductSummaryService(
    IDbContextFactory<StockroomDbContext> factory, HybridCache cache)
{
    private const string Tag = "product-summary";

    // Even a missed clear heals itself after ten minutes.
    private static readonly HybridCacheEntryOptions Options = new()
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
    };

    public ValueTask<ProductSummary> SummarizeAsync(
        StockLevel? level,
        CancellationToken cancellationToken = default)
    {
        return cache.GetOrCreateAsync(
            $"product-summary:{level?.ToString() ?? "All"}",
            ct => ComputeAsync(level, ct),
            Options,
            [Tag],
            cancellationToken);
    }

    // No cancellation token on purpose: the write has happened,
    // so the clear must happen too.
    public ValueTask InvalidateAsync()
    {
        return cache.RemoveByTagAsync(Tag);
    }

    private async ValueTask<ProductSummary> ComputeAsync(
        StockLevel? level, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var all = await db.Products.AsNoTracking().ToListAsync(ct);

        return ProductSummary.Create(
            StockRules.FilterByLevel(all, level));
    }
}
