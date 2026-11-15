using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

/// <summary>
/// Sample rows for development. A row is found by its business key,
/// never by its Id, so a sample row can never take a user's key.
/// </summary>
public static class SampleData
{
    public static void Seed(DbContext db, bool storeChanged)
    {
        // The EF Core tools call this synchronous version. They run
        // without a synchronization context, so waiting is safe.
        SeedAsync(db, storeChanged, CancellationToken.None)
            .GetAwaiter().GetResult();
    }

    public static async Task SeedAsync(
        DbContext db, bool storeChanged, CancellationToken token)
    {
        var products = db.Set<Product>();
        var skus = await products.Select(p => p.Sku)
            .ToListAsync(token);
        products.AddRange(
            CreateProducts().Where(p => !skus.Contains(p.Sku)));

        var suppliers = db.Set<Supplier>();
        var names = await suppliers.Select(s => s.Name)
            .ToListAsync(token);
        suppliers.AddRange(
            CreateSuppliers().Where(s => !names.Contains(s.Name)));

        await db.SaveChangesAsync(token);
    }

    private static List<Product> CreateProducts() =>
    [
        new() { Sku = "SH-120", Name = "Steel shelf 120 cm",
            PriceCents = 8900, StockQuantity = 14 },
        new() { Sku = "PJ-200", Name = "Pallet jack",
            PriceCents = 34900, StockQuantity = 3 },
        new() { Sku = "TP-050", Name = "Packing tape 50 m",
            PriceCents = 249, StockQuantity = 240 },
        new() { Sku = "BX-S", Name = "Cardboard box, small",
            PriceCents = 85, StockQuantity = 0 },
        new() { Sku = "BX-L", Name = "Cardboard box, large",
            PriceCents = 145, StockQuantity = 120 },
        new() { Sku = "GL-M", Name = "Safety gloves, size M",
            PriceCents = 490, StockQuantity = 8 },
        new() { Sku = "SF-500", Name = "Stretch film 500 mm",
            PriceCents = 1290, StockQuantity = 36 },
        new() { Sku = "LP-01", Name = "Label printer",
            PriceCents = 18900, StockQuantity = 0, IsActive = false },
    ];

    private static List<Supplier> CreateSuppliers() =>
    [
        new() { Name = "Brightway Packaging",
            Email = "orders@brightway.example" },
        new() { Name = "Kestrel Lifting Gear",
            Email = "sales@kestrel.example" },
        new() { Name = "Meadow Safety Supplies",
            Email = "hello@meadow-safety.example" },
        new() { Name = "Old Mill Labels",
            Email = "office@oldmill.example", IsActive = false },
    ];
}
