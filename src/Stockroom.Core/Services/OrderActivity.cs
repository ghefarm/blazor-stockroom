using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;

namespace Stockroom.Core.Services;

public class OrderActivity(
    IDbContextFactory<StockroomDbContext> factory,
    BusinessClock clock)
{
    public async Task<int> CountOrderedThisMonthAsync(
        CancellationToken ct = default)
    {
        var today = clock.Today;
        var first = new DateOnly(today.Year, today.Month, 1);
        var next = first.AddMonths(1);

        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.PurchaseOrders.CountAsync(
            o => o.OrderedOn >= first && o.OrderedOn < next, ct);
    }
}
