using Stockroom.Core.Entities;

namespace Stockroom.Core.Sample;

// In-memory data until Stockroom has a database.
public static class SampleProducts
{
    public static IReadOnlyList<Product> All { get; } =
    [
        new() { Id = 1, Sku = "SH-120", Name = "Steel shelf 120 cm",
            PriceCents = 8900, StockQuantity = 14, IsActive = true },
        new() { Id = 2, Sku = "PJ-200", Name = "Pallet jack",
            PriceCents = 34900, StockQuantity = 3, IsActive = true },
        new() { Id = 3, Sku = "TP-050", Name = "Packing tape 50 m",
            PriceCents = 249, StockQuantity = 240, IsActive = true },
        new() { Id = 4, Sku = "BX-S", Name = "Cardboard box, small",
            PriceCents = 85, StockQuantity = 0, IsActive = true },
        new() { Id = 5, Sku = "BX-L", Name = "Cardboard box, large",
            PriceCents = 145, StockQuantity = 120, IsActive = true },
        new() { Id = 6, Sku = "GL-M", Name = "Safety gloves, size M",
            PriceCents = 490, StockQuantity = 8, IsActive = true },
        new() { Id = 7, Sku = "SF-500", Name = "Stretch film 500 mm",
            PriceCents = 1290, StockQuantity = 36, IsActive = true },
        new() { Id = 8, Sku = "LP-01", Name = "Label printer",
            PriceCents = 18900, StockQuantity = 0, IsActive = false },
    ];

    public static Product? FindById(int id) =>
        All.FirstOrDefault(p => p.Id == id);
}
