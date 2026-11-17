namespace Stockroom.Core;

/// <summary>
/// How many products are at each stock level.
/// </summary>
public sealed record StockCounts(int InStock, int Low, int OutOfStock)
{
    public int Total => InStock + Low + OutOfStock;
}
