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

    public async Task<int> CountOpenAsync(
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.PurchaseOrders.CountAsync(
            o => o.Status == OrderStatus.Open
                || o.Status == OrderStatus.PartlyDelivered,
            ct);
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
