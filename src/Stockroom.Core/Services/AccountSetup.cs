using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

/// <summary>
/// Runs at every start: makes sure the roles and a first
/// administrator exist.
/// </summary>
public class AccountSetup(
    RoleManager<IdentityRole> roles,
    UserManager<ApplicationUser> users)
{
    public async Task EnsureRolesAsync()
    {
        // At every start, so a role added in a later version also
        // appears in an existing database.
        foreach (var name in Roles.All)
        {
            if (!await roles.RoleExistsAsync(name))
            {
                await roles.CreateAsync(new IdentityRole(name));
            }
        }
    }

    /// <summary>
    /// Creates the first administrator when the database has no
    /// users. Returns false when there was nothing to do.
    /// </summary>
    public async Task<bool> EnsureFirstAdminAsync(
        string userName, string password)
    {
        if (await users.Users.AnyAsync())
        {
            return false;
        }

        var admin = new ApplicationUser
        {
            UserName = userName,
            DisplayName = "Administrator",
            MustChangePassword = true,
        };
        var created = await users.CreateAsync(admin, password);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException(
                "The first administrator could not be created: "
                + string.Join(" ", created.Errors
                    .Select(e => e.Description)));
        }

        await users.AddToRoleAsync(admin, Roles.Admin);
        return true;
    }

    public async Task<bool> HasAdminWithInitialPasswordAsync()
    {
        var admins = await users.GetUsersInRoleAsync(Roles.Admin);
        return admins.Any(a => a.MustChangePassword && !a.IsArchived);
    }
}
