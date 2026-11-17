using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

/// <summary>
/// Accounts as the administration pages see them. Reads use their own
/// short-lived context, changes go through Identity's UserManager.
/// </summary>
public class UserAccountService(
    UserManager<ApplicationUser> users,
    IDbContextFactory<StockroomDbContext> factory)
{
    public Task<IReadOnlyList<AccountSummary>> ListAsync(
        CancellationToken ct = default) => QueryAsync(null, ct);

    /// <summary>
    /// All accounts for an administrator, only the own account for
    /// everybody else.
    /// </summary>
    public Task<IReadOnlyList<AccountSummary>> ListVisibleAsync(
        AccountViewer viewer, CancellationToken ct = default) =>
        QueryAsync(viewer.IsAdmin ? null : viewer.UserId, ct);

    public async Task<AccountSummary?> FindAsync(
        string userId, AccountViewer viewer,
        CancellationToken ct = default)
    {
        // The page already sends such a user to the own account. A
        // call that still arrives here is a programming error.
        if (!viewer.IsAdmin && userId != viewer.UserId)
        {
            throw new InvalidOperationException(
                "Only an administrator may read another account.");
        }

        var found = await QueryAsync(userId, ct);
        return found.SingleOrDefault();
    }
    public static IReadOnlyList<string> NormalizeRoles(
        IEnumerable<string> roles)
    {
        var chosen = roles.Distinct().ToList();

        // The administrator role includes every area, so it is
        // stored alone.
        return chosen.Contains(Roles.Admin) ? [Roles.Admin] : chosen;
    }

    public async Task<AccountResult> CreateAsync(NewAccount account)
    {
        var roles = NormalizeRoles(account.Roles);
        var unknown = roles.Except(Roles.All).ToList();
        if (unknown.Count > 0)
        {
            return AccountResult.Fail(
                $"Unknown role: {string.Join(", ", unknown)}.");
        }

        var user = new ApplicationUser
        {
            UserName = account.UserName.Trim(),
            DisplayName = account.DisplayName.Trim(),
            MustChangePassword = true,
        };
        var password = PasswordGenerator.Create();
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return Fail(created);
        }

        var complete = false;
        try
        {
            var added = await users.AddToRolesAsync(user, roles);
            complete = added.Succeeded;
            return complete
                ? AccountResult.Ok(password)
                : Fail(added);
        }
        finally
        {
            // Never leave a user behind without the roles that the
            // administrator chose.
            if (!complete)
            {
                await users.DeleteAsync(user);
            }
        }
    }

    public async Task<AccountResult> ResetPasswordAsync(string userId)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is null)
        {
            return AccountResult.Fail("The account does not exist.");
        }

        var password = PasswordGenerator.Create();
        var token = await users.GeneratePasswordResetTokenAsync(user);

        // Saved together with the new password hash.
        user.MustChangePassword = true;
        var reset = await users.ResetPasswordAsync(
            user, token, password);
        return reset.Succeeded
            ? AccountResult.Ok(password)
            : Fail(reset);
    }

    public async Task<AccountResult> ChangePasswordAsync(
        ApplicationUser user, string current, string next)
    {
        if (string.Equals(current, next, StringComparison.Ordinal))
        {
            return AccountResult.Fail(
                "The new password must differ from the old one.");
        }

        var changed =
            await users.ChangePasswordAsync(user, current, next);
        if (!changed.Succeeded)
        {
            return Fail(changed);
        }

        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            await users.UpdateAsync(user);
        }

        return AccountResult.Ok();
    }

    public async Task<AccountResult> ArchiveAsync(string userId)
    {
        var user = await users.FindByIdAsync(userId);
        if (user is null || user.IsArchived)
        {
            return AccountResult.Fail("The account does not exist.");
        }

        if (await IsLastActiveAdminAsync(user))
        {
            return AccountResult.Fail(
                "The last administrator cannot be archived.");
        }

        user.IsArchived = true;

        // A new security stamp ends every open sign-in of this user
        // at the next check. It also saves IsArchived.
        var updated = await users.UpdateSecurityStampAsync(user);
        return updated.Succeeded ? AccountResult.Ok() : Fail(updated);
    }

    private async Task<bool> IsLastActiveAdminAsync(
        ApplicationUser user)
    {
        if (!await users.IsInRoleAsync(user, Roles.Admin))
        {
            return false;
        }

        var admins = await users.GetUsersInRoleAsync(Roles.Admin);
        return admins.Count(a => !a.IsArchived) <= 1;
    }

    private async Task<IReadOnlyList<AccountSummary>> QueryAsync(
        string? onlyUserId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var accounts = await db.Users.AsNoTracking()
            .Where(u => onlyUserId == null || u.Id == onlyUserId)
            .OrderBy(u => u.UserName)
            .ToListAsync(ct);
        var ids = accounts.Select(u => u.Id).ToList();
        var roles = await (
            from ur in db.UserRoles
            join r in db.Roles on ur.RoleId equals r.Id
            where ids.Contains(ur.UserId)
            select new { ur.UserId, r.Name }).ToListAsync(ct);

        return [.. accounts.Select(u => new AccountSummary(
            u.Id,
            u.UserName ?? "",
            u.DisplayName,
            [.. roles.Where(r => r.UserId == u.Id)
                .Select(r => r.Name!).Order()],
            u.MustChangePassword,
            u.IsArchived))];
    }

    private static AccountResult Fail(IdentityResult result) =>
        AccountResult.Fail(
            [.. result.Errors.Select(e => e.Description)]);
}
