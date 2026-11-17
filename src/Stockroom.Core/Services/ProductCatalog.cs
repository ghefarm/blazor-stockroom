using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;
using Stockroom.Core.ReadOnly;

namespace Stockroom.Core.Services;

// Every method that writes products also clears the summaries.
// A caller cannot forget it, because the caller never does it.
public class ProductCatalog(
    IDbContextFactory<StockroomDbContext> factory,
    ProductSummaryService summaries)
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // Without ORDER BY, SQL promises no order at all.
        return await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .ToListAsync(ct);
    }

    public async Task<ProductSlice> GetSliceAsync(
        int start, int count, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var total = await db.Products.CountAsync(ct);
        var items = await db.Products
            .AsNoTracking()
            .OrderBy(p => p.Id)
            .Skip(start)
            .Take(count)
            .ToListAsync(ct);

        return new ProductSlice(items, total);
    }

    public async Task<Product?> FindByIdAsync(
        int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // Not FindAsync: it always tracks the entity it returns.
        return await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<SaveOutcome> ChangePriceAsync(
        int id, long priceCents, int version,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return SaveOutcome.NotFound;
        }

        // The version the user saw, not the one just loaded, so a
        // change since the form opened makes the UPDATE miss.
        db.Entry(product).Property(p => p.Version).OriginalValue =
            version;
        product.Version = version + 1;
        product.PriceCents = priceCents;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return SaveOutcome.Conflict;
        }

        await summaries.InvalidateAsync();
        return SaveOutcome.Saved;
    }

    public async Task<bool> ChangeStockQuantityAsync(
        int id, int quantity, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return false;
        }

        product.StockQuantity = quantity;

        // Every write adds 1, so an open price form learns of it.
        product.Version++;
        await db.SaveChangesAsync(ct);
        await summaries.InvalidateAsync();
        return true;
    }

    public async Task<bool> ChangeNameAsync(
        int id, string name, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return false;
        }

        product.Name = name.Trim();
        product.Version++;
        await db.SaveChangesAsync(ct);
        await summaries.InvalidateAsync();
        return true;
    }

    public async Task<BatchResult> ChangePricesAsync(
        IReadOnlyList<PriceChange> changes,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var result = await ApplyPricesAsync(db, changes, ct);
        await db.SaveChangesAsync(ct);
        await summaries.InvalidateAsync();
        return result;
    }

    public async Task<int> DeactivateAsync(
        IReadOnlyList<string> skus, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // ExecuteUpdate never passes SaveChanges or its interceptor.
        db.ThrowIfReadOnly();

        // No change tracker here: the version must be set by hand,
        // or an open form would overwrite this change unnoticed.
        var changed = await db.Products
            .Where(p => skus.Contains(p.Sku) && p.IsActive)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsActive, false)
                .SetProperty(p => p.Version, p => p.Version + 1),
                ct);
        await summaries.InvalidateAsync();
        return changed;
    }

    /// <summary>
    /// Changes the prices in the context and leaves the save to the
    /// caller, so a caller can add more rows to the same save.
    /// </summary>
    internal static async Task<BatchResult> ApplyPricesAsync(
        StockroomDbContext db,
        IReadOnlyList<PriceChange> changes,
        CancellationToken ct)
    {
        string[] skus = [.. changes.Select(c => c.Sku)];

        // One query for the whole set, not one per row.
        var bySku = await db.Products
            .Where(p => skus.Contains(p.Sku))
            .ToDictionaryAsync(p => p.Sku, ct);

        var changed = 0;
        foreach (var change in changes)
        {
            if (!bySku.TryGetValue(change.Sku, out var product)
                || product.PriceCents == change.PriceCents)
            {
                continue;
            }

            product.PriceCents = change.PriceCents;
            product.Version++;
            changed++;
        }

        return new BatchResult(changed, changes.Count - changed);
    }
}
