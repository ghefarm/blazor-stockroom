using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

public class OrderListService(
    IDbContextFactory<StockroomDbContext> factory)
{
    public static IReadOnlyList<int> PageSizes { get; } =
        [10, 25, 50];

    public async Task<OrderListPage> LoadAsync(
        OrderListFilter requested, int page, int pageSize,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var suppliers = await db.Suppliers
            .OrderBy(s => s.Name)
            .Select(s => new SupplierOption(s.Id, s.Name))
            .ToListAsync(ct);
        var filter = Apply(requested.Normalize(), suppliers);

        // The tabs count each status under all the other filters.
        var counts = await Filter(db, filter with { Status = null })
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(c => c.Status, c => c.Count, ct);
        var total = filter.Status is { } status
            ? counts.GetValueOrDefault(status)
            : counts.Values.Sum();

        var size = PageSizes.Contains(pageSize)
            ? pageSize
            : PageSizes[0];
        var applied = Math.Clamp(
            page, 1, OrderListPage.CountPages(total, size));
        var rows = await Summarize(db, Filter(db, filter))
            .Skip((applied - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return new OrderListPage(
            filter, rows, applied, size, total, suppliers, counts);
    }

    public async Task<IReadOnlyList<OrderSummary>> ExportAsync(
        OrderListFilter filter, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var query = Filter(db, filter.Normalize());
        return await Summarize(db, query).ToListAsync(ct);
    }

    private static OrderListFilter Apply(
        OrderListFilter filter, List<SupplierOption> suppliers)
    {
        // The dropdown offers only existing suppliers. An unknown id
        // would show "All" and still filter, so it is dropped.
        if (filter.SupplierId is { } id
            && !suppliers.Exists(s => s.Id == id))
        {
            return filter with { SupplierId = null };
        }

        return filter;
    }

    // The one place that decides which orders a filter means.
    private static IQueryable<PurchaseOrder> Filter(
        StockroomDbContext db, OrderListFilter filter)
    {
        var query = db.PurchaseOrders.AsQueryable();
        if (filter.SupplierId is { } supplierId)
        {
            query = query.Where(o => o.SupplierId == supplierId);
        }

        if (filter.Status is { } status)
        {
            query = query.Where(o => o.Status == status);
        }

        if (filter.From is { } from)
        {
            query = query.Where(o => o.OrderedOn >= from);
        }

        if (filter.To is { } to)
        {
            query = query.Where(o => o.OrderedOn <= to);
        }

        if (filter.Search is { } search)
        {
            var text = search.ToLowerInvariant();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(text)
                || o.Lines.Any(l => db.Products.Any(p =>
                    p.Id == l.ProductId
                    && p.Name.ToLower().Contains(text))));
        }

        return query;
    }

    private static IQueryable<OrderSummary> Summarize(
        StockroomDbContext db, IQueryable<PurchaseOrder> orders) =>
        from o in orders
        join s in db.Suppliers on o.SupplierId equals s.Id
        // The order number makes the order unique, so paging never
        // shows a row twice or skips one.
        orderby o.OrderedOn descending, o.OrderNumber
        select new OrderSummary(
            o.Id,
            o.OrderNumber,
            s.Name,
            o.OrderedOn,
            o.Status,
            o.Lines.Sum(l => l.Quantity * l.UnitPriceCents));
}
