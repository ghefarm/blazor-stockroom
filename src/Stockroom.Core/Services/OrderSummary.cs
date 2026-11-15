using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

/// <summary>
/// One row of the order list: only what the list shows.
/// </summary>
public sealed record OrderSummary(
    int Id,
    string OrderNumber,
    string SupplierName,
    DateOnly OrderedOn,
    OrderStatus Status,
    long TotalCents);
