using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

public class ProductCatalog(
    IDbContextFactory<StockroomDbContext> factory)
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

    public async Task<Product?> FindByIdAsync(
        int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // Not FindAsync: it always tracks the entity it returns.
        return await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<bool> ChangePriceAsync(
        int id, long priceCents, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
        {
            return false;
        }

        product.PriceCents = priceCents;
        await db.SaveChangesAsync(ct);
        return true;
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
        await db.SaveChangesAsync(ct);
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
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<BatchResult> ChangePricesAsync(
        IReadOnlyList<PriceChange> changes,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
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
            changed++;
        }

        await db.SaveChangesAsync(ct);
        return new BatchResult(changed, changes.Count - changed);
    }
}
