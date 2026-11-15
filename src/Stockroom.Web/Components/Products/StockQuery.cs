using Stockroom.Core.Entities;

namespace Stockroom.Web.Components.Products;

// The stock filter as it appears in the address. "All" has its own
// word, so an address without the parameter stays free for a
// different default.
public static class StockQuery
{
    public const string Name = "stock";

    public static string Format(StockLevel? level) => level switch
    {
        null => "all",
        StockLevel.InStock => "in",
        StockLevel.Low => "low",
        _ => "out",
    };

    public static StockLevel? Parse(string? value) => value switch
    {
        "in" => StockLevel.InStock,
        "low" => StockLevel.Low,
        "out" => StockLevel.OutOfStock,
        _ => null,
    };

    public static string CreateLink(StockLevel? level) =>
        $"products?{Name}={Format(level)}";
}
