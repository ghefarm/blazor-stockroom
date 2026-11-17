namespace Stockroom.Core.Services;

/// <summary>
/// One row of the order list with the names of its products.
/// </summary>
public sealed record OrderRow(
    OrderSummary Order,
    IReadOnlyList<string> ProductNames);
