using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Auditing;

/// <summary>
/// Fills the audit fields of every added or changed entity before
/// EF Core writes it, whichever SaveChanges method was called.
/// </summary>
public sealed class AuditInterceptor(
    ICurrentUser currentUser, TimeProvider time)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampAuditFields(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        StampAuditFields(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void StampAuditFields(DbContext? db)
    {
        if (db is null)
        {
            return;
        }

        var entries = db.ChangeTracker.Entries<IAuditable>()
            .Where(e => e.State is EntityState.Added
                or EntityState.Modified)
            .ToList();
        if (entries.Count == 0)
        {
            return;
        }

        var now = time.GetUtcNow().UtcDateTime;
        var user = currentUser.GetName();
        foreach (var entry in entries)
        {
            Stamp(entry, now, user);
        }
    }

    private static void Stamp(
        EntityEntry<IAuditable> entry, DateTime now, string user)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreatedAt = now;
            entry.Entity.CreatedBy = user;
            return;
        }

        entry.Entity.LastModifiedAt = now;
        entry.Entity.LastModifiedBy = user;

        // Whatever the caller sent, an update never rewrites who
        // created the row.
        entry.Property(e => e.CreatedAt).IsModified = false;
        entry.Property(e => e.CreatedBy).IsModified = false;
    }
}
