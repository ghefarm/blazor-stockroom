using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

/// <summary>
/// Everything the order list shows for one filter: the rows of one
/// page, the choices of the dropdown, the counts of the tabs, and
/// the filter and page that were really applied.
/// </summary>
public sealed record OrderListPage(
    OrderListFilter Filter,
    IReadOnlyList<OrderSummary> Rows,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<SupplierOption> Suppliers,
    IReadOnlyDictionary<OrderStatus, int> StatusCounts)
{
    public int PageCount => CountPages(TotalCount, PageSize);

    public int FirstRow => TotalCount == 0
        ? 0
        : (Page - 1) * PageSize + 1;

    public int LastRow => Math.Min(Page * PageSize, TotalCount);

    public static int CountPages(int total, int pageSize) =>
        Math.Max(1, (total + pageSize - 1) / pageSize);

    // The first row on the screen stays on the screen when the
    // user picks another page size.
    public int FindPageFor(int newSize) =>
        (Page - 1) * PageSize / newSize + 1;
}

public sealed record SupplierOption(int Id, string Name);
