using Stockroom.Core.Entities;

namespace Stockroom.Core;

public sealed record ReorderLine(Product Product, int Quantity)
{
    public long CostCents => Product.PriceCents * Quantity;
}

public static class ReorderPlanner
{
    // An invented rule: fill every active product up to one target.
    // Inactive products are no longer bought, so they never appear.
    public static IReadOnlyList<ReorderLine> CreatePlan(
        IEnumerable<Product> products, int fillUpTo)
    {
        var lines = products
            .Where(p => p.IsActive && p.StockQuantity < fillUpTo)
            .Select(p => new ReorderLine(
                p, fillUpTo - p.StockQuantity));

        return [.. lines];
    }
}
