using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class AccountVisibilityTests
{
    [Test]
    public async Task ShowsAdminsEveryAccountAndOthersTheirOwn()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var scope = database.CreateIdentityScope();
        var (accounts, mara, tom) =
            await CreateTwoAccountsAsync(scope);
        var admin = new AccountViewer(mara.Id, IsAdmin: true);
        var clerk = new AccountViewer(tom.Id, IsAdmin: false);

        var forAdmin = await accounts.ListVisibleAsync(admin);
        var forClerk = await accounts.ListVisibleAsync(clerk);
        var other = await accounts.FindAsync(tom.Id, admin);
        var own = await accounts.FindAsync(tom.Id, clerk);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(forAdmin.Select(a => a.UserName),
                Is.EqualTo(new[] { "mara", "tom" }));
            Assert.That(forClerk.Select(a => a.UserName),
                Is.EqualTo(new[] { "tom" }));
            Assert.That(other!.Roles,
                Is.EqualTo(new[] { "Warehouse" }));
            Assert.That(own!.UserName, Is.EqualTo("tom"));
        }
    }

    [Test]
    public async Task RefusesAnotherAccountToSomeoneWhoIsNoAdmin()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var scope = database.CreateIdentityScope();
        var (accounts, mara, tom) =
            await CreateTwoAccountsAsync(scope);
        var clerk = new AccountViewer(tom.Id, IsAdmin: false);
        var admin = new AccountViewer(mara.Id, IsAdmin: true);

        var unknown = await accounts.FindAsync("nobody", admin);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(unknown, Is.Null);
            Assert.ThrowsAsync<InvalidOperationException>(
                () => accounts.FindAsync(mara.Id, clerk));
        }
    }

    private static async Task<(UserAccountService, ApplicationUser,
        ApplicationUser)> CreateTwoAccountsAsync(
        AsyncServiceScope scope)
    {
        var services = scope.ServiceProvider;
        await services.GetRequiredService<AccountSetup>()
            .EnsureRolesAsync();
        var accounts =
            services.GetRequiredService<UserAccountService>();
        var users = services
            .GetRequiredService<UserManager<ApplicationUser>>();
        await accounts.CreateAsync(
            new NewAccount("mara", "Mara Quill", [Roles.Admin]));
        await accounts.CreateAsync(
            new NewAccount("tom", "Tom Birch", [Roles.Warehouse]));
        return (accounts, (await users.FindByNameAsync("mara"))!,
            (await users.FindByNameAsync("tom"))!);
    }
}
