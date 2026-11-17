using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class AccountSetupTests
{
    [Test]
    public async Task CreatesRolesAndTheFirstAdminOnce()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var scope = database.CreateIdentityScope();
        var setup = scope.ServiceProvider
            .GetRequiredService<AccountSetup>();

        await setup.EnsureRolesAsync();
        var first = await setup.EnsureFirstAdminAsync(
            "admin", "First-Start-2026");
        await setup.EnsureRolesAsync();
        var second = await setup.EnsureFirstAdminAsync(
            "other", "First-Start-2026");
        var reminder = await setup.HasAdminWithInitialPasswordAsync();

        await using var db = database.CreateContext();
        var roles = await db.Roles.Select(r => r.Name).ToListAsync();
        var users = await db.Users.Select(u => u.UserName)
            .ToListAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(roles, Is.EquivalentTo(Roles.All));
            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(users, Is.EqualTo(new[] { "admin" }));
            Assert.That(reminder, Is.True);
        }
    }

    [Test]
    public async Task RefusesAFirstAdminWithAWeakPassword()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var scope = database.CreateIdentityScope();
        var setup = scope.ServiceProvider
            .GetRequiredService<AccountSetup>();
        await setup.EnsureRolesAsync();

        var error = Assert.ThrowsAsync<InvalidOperationException>(
            () => setup.EnsureFirstAdminAsync("admin", "short"));

        await using var db = database.CreateContext();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(error!.Message,
                Does.Contain("at least 10 characters"));
            Assert.That(await db.Users.AnyAsync(), Is.False);
        }
    }
}
