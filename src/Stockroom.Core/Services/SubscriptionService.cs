using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;
using Stockroom.Core.ReadOnly;

namespace Stockroom.Core.Services;

public class SubscriptionService(
    IDbContextFactory<StockroomDbContext> factory,
    SubscriptionState state)
{
    public async Task LoadAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var row = await db.Subscriptions.AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.Id == Subscription.SingleId, ct);
        state.Set(row?.PaidUntil);
    }

    public async Task ExtendAsync(
        DateOnly paidUntil, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var row = await db.Subscriptions.FirstOrDefaultAsync(
            s => s.Id == Subscription.SingleId, ct);
        if (row is null)
        {
            row = new Subscription();
            db.Subscriptions.Add(row);
        }

        row.PaidUntil = paidUntil;
        await db.SaveChangesAsync(ct);
        state.Set(paidUntil);
    }
}
