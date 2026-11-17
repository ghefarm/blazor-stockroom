using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

/// <summary>
/// Which orders the user wants to see. The table, the export and
/// every other consumer of the order list share this one record.
/// </summary>
public sealed record OrderListFilter(
    int? SupplierId = null,
    OrderStatus? Status = null,
    DateOnly? From = null,
    DateOnly? To = null,
    string? Search = null)
{
    public static OrderListFilter Empty { get; } = new();

    // Decided once here, so the query, the file name and the page
    // never trim the text each in their own way.
    public OrderListFilter Normalize() => this with
    {
        Search = string.IsNullOrWhiteSpace(Search)
            ? null
            : Search.Trim(),
    };
}
