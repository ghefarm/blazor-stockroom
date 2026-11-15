using Stockroom.Core.Entities;

namespace Stockroom.Web.Components.Products;

public static class StockText
{
    public static string Describe(StockLevel level) => level switch
    {
        StockLevel.Low => "Low stock",
        StockLevel.OutOfStock => "Out of stock",
        _ => "In stock",
    };

    public static string DescribeFilter(StockLevel? level) =>
        level is null ? "All" : Describe(level.Value);
}
