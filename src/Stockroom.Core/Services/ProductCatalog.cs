using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

public class ProductCatalog(
    IDbContextFactory<StockroomDbContext> factory)
{
    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();

        // Without ORDER BY, SQL promises no order at all.
        return await db.Products
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<Product?> FindByIdAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Products.FindAsync(id);
    }

    public async Task<bool> ChangeStockQuantityAsync(
        int id, int quantity)
    {
        await using var db = await factory.CreateDbContextAsync();
        var product = await db.Products.FindAsync(id);
        if (product is null)
        {
            return false;
        }

        product.StockQuantity = quantity;
        await db.SaveChangesAsync();
        return true;
    }
}
