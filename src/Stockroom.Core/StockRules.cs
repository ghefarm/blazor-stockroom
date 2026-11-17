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

    public static StockCounts CountLevels(
        IEnumerable<Product> products)
    {
        int inStock = 0, low = 0, outOfStock = 0;
        foreach (var product in products)
        {
            switch (DetermineLevel(product.StockQuantity))
            {
                case StockLevel.Low:
                    low++;
                    break;
                case StockLevel.OutOfStock:
                    outOfStock++;
                    break;
                default:
                    inStock++;
                    break;
            }
        }

        return new StockCounts(inStock, low, outOfStock);
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
