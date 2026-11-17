using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

public class PurchaseOrderService(
    IDbContextFactory<StockroomDbContext> factory)
{
    public async Task<IReadOnlyList<OrderSummary>> ListAsync(
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        var query =
            from o in db.PurchaseOrders
            join s in db.Suppliers on o.SupplierId equals s.Id
            orderby o.OrderedOn descending, o.OrderNumber
            select new OrderSummary(
                o.Id,
                o.OrderNumber,
                s.Name,
                o.OrderedOn,
                o.Status,
                o.Lines.Sum(l => l.Quantity * l.UnitPriceCents));

        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrderRow>> ListWithProductsAsync(
        CancellationToken ct = default)
    {
        var orders = await ListAsync(ct);
        return await AddProductNamesAsync(orders, ct);
    }

    /// <summary>
    /// Adds the product names to rows that are already loaded, such
    /// as one page of the order list.
    /// </summary>
    public async Task<IReadOnlyList<OrderRow>> AddProductNamesAsync(
        IReadOnlyList<OrderSummary> orders,
        CancellationToken ct = default)
    {
        var names = await FindProductNamesAsync(
            [.. orders.Select(o => o.Id)], ct);

        return [.. orders.Select(
            o => new OrderRow(o, [.. names[o.Id]]))];
    }

    private async Task<ILookup<int, string>> FindProductNamesAsync(
        int[] orderIds, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // One query for the products of all rows, not one per row.
        var lines = await (
            from l in db.PurchaseOrderLines
            join p in db.Products on l.ProductId equals p.Id
            where orderIds.Contains(l.PurchaseOrderId)
            orderby p.Name
            select new { l.PurchaseOrderId, p.Name })
            .ToListAsync(ct);

        return lines.ToLookup(l => l.PurchaseOrderId, l => l.Name);
    }

    public async Task<IReadOnlyList<string>> GetProductNamesAsync(
        int orderId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var query =
            from l in db.PurchaseOrderLines
            join p in db.Products on l.ProductId equals p.Id
            where l.PurchaseOrderId == orderId
            orderby p.Name
            select p.Name;

        return await query.ToListAsync(ct);
    }

    public async Task<int> CountOpenAsync(
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.PurchaseOrders.CountAsync(
            o => o.Status == OrderStatus.Open
                || o.Status == OrderStatus.PartlyDelivered,
            ct);
    }

    public async Task<bool> HasOpenOrdersAsync(
        int supplierId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // Any stops at the first match. A count would read them all.
        return await db.PurchaseOrders.AnyAsync(
            o => o.SupplierId == supplierId
                && (o.Status == OrderStatus.Open
                    || o.Status == OrderStatus.PartlyDelivered),
            ct);
    }

    public async Task<SupplierOverview?> GetSupplierOverviewAsync(
        int supplierId, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        var query = db.Suppliers
            .Where(s => s.Id == supplierId)
            .Select(s => new
            {
                s.Name,
                Orders = db.PurchaseOrders
                    .Where(o => o.SupplierId == s.Id)
                    .OrderBy(o => o.OrderNumber)
                    .Select(o => o.OrderNumber)
                    .ToList(),
                Products = (
                    from o in db.PurchaseOrders
                    where o.SupplierId == s.Id
                    from l in o.Lines
                    join p in db.Products on l.ProductId equals p.Id
                    orderby p.Name
                    select p.Name).ToList(),
            });

        // Two collections in one query would repeat every order
        // once for every line. Three small queries instead.
        var found = await query.AsSplitQuery()
            .FirstOrDefaultAsync(ct);
        if (found is null)
        {
            return null;
        }

        // Distinct inside the query needs APPLY, which SQLite lacks.
        return new SupplierOverview(
            found.Name, found.Orders, [.. found.Products.Distinct()]);
    }

    public async Task<bool> CancelAsync(
        int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // Loaded only while it is still open. A double click or a
        // second browser then finds nothing and changes nothing.
        var order = await db.PurchaseOrders.FirstOrDefaultAsync(
            o => o.Id == id && o.Status == OrderStatus.Open, ct);
        if (order is null)
        {
            return false;
        }

        order.Status = OrderStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
