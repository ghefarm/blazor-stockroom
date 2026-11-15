using Stockroom.Core.Entities;

namespace Stockroom.Core;

public static class StockRules
{
    // One limit for the whole app, so a badge and a filter never
    // disagree about what "low" means.
    public const int LowLimit = 10;

    public static StockLevel DetermineLevel(int quantity)
    {
        if (quantity <= 0)
        {
            return StockLevel.OutOfStock;
        }

        if (quantity < LowLimit)
        {
            return StockLevel.Low;
        }

        return StockLevel.InStock;
    }

    public static IReadOnlyList<Product> FilterByLevel(
        IEnumerable<Product> products, StockLevel? level)
    {
        if (level is null)
        {
            return [.. products];
        }

        var matching = products.Where(
            p => DetermineLevel(p.StockQuantity) == level);

        return [.. matching];
    }
}
