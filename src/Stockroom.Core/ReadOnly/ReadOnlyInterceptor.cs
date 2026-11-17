using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stockroom.Core.Entities;

namespace Stockroom.Core.ReadOnly;

/// <summary>
/// Refuses every save of business data while no paid subscription
/// covers today.
/// </summary>
public sealed class ReadOnlyInterceptor(SubscriptionState state)
    : SaveChangesInterceptor
{
    // While the application is read-only, users must still enter a
    // new subscription, sign in, change a password and set up
    // two-factor sign-in, and the start must create the roles and
    // the first administrator. Identity writes all of its tables.
    private static readonly HashSet<Type> AlwaysWritable =
    [
        typeof(Subscription),
        typeof(ApplicationUser),
        typeof(IdentityRole),
        typeof(IdentityUserRole<string>),
        typeof(IdentityUserClaim<string>),
        typeof(IdentityRoleClaim<string>),
        typeof(IdentityUserLogin<string>),
        typeof(IdentityUserToken<string>),
        typeof(IdentityUserPasskey<string>),
    ];

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ThrowIfBlocked(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        ThrowIfBlocked(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public void ThrowIfReadOnly()
    {
        if (!state.AllowsWrites)
        {
            throw new ReadOnlyModeException();
        }
    }

    private void ThrowIfBlocked(DbContext? context)
    {
        // The date comparison is cheap. The change tracker is only
        // read when the answer is no.
        if (state.AllowsWrites || context is null)
        {
            return;
        }

        var blocked = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added
                or EntityState.Modified or EntityState.Deleted)
            .Any(e => !AlwaysWritable.Contains(e.Metadata.ClrType));
        if (blocked)
        {
            throw new ReadOnlyModeException();
        }
    }
}
