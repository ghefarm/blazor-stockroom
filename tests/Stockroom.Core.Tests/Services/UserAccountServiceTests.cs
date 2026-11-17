using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class UserAccountServiceTests
{
    [Test]
    public async Task CreatesChangesResetsAndArchivesAccounts()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var scope = database.CreateIdentityScope();
        var (accounts, users) = await PrepareAsync(scope);

        var created = await accounts.CreateAsync(new NewAccount(
            "mara", "Mara Quill", [Roles.Purchasing, Roles.Admin]));
        var mara = (await users.FindByNameAsync("mara"))!;
        var flagAfterCreate = mara.MustChangePassword;
        var changed = await accounts.ChangePasswordAsync(
            mara, created.Password!, "Own-Choice-2026");
        var flagAfterChange = mara.MustChangePassword;
        var reset = await accounts.ResetPasswordAsync(mara.Id);

        var clerk = await accounts.CreateAsync(new NewAccount(
            "tom", "Tom Birch", [Roles.Warehouse]));
        var tom = (await users.FindByNameAsync("tom"))!;
        var stamp = tom.SecurityStamp;
        var archived = await accounts.ArchiveAsync(tom.Id);
        var list = await accounts.ListAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(created.Succeeded, Is.True);
            Assert.That(created.Password, Has.Length.EqualTo(16));
            Assert.That(flagAfterCreate, Is.True);
            Assert.That(changed.Succeeded, Is.True);
            Assert.That(flagAfterChange, Is.False);
            Assert.That(reset.Password,
                Is.Not.EqualTo(created.Password));
            Assert.That(mara.MustChangePassword, Is.True);
            Assert.That(await users.CheckPasswordAsync(
                mara, reset.Password!), Is.True);
            Assert.That(clerk.Succeeded, Is.True);
            Assert.That(archived.Succeeded, Is.True);
            Assert.That(tom.SecurityStamp, Is.Not.EqualTo(stamp));
            Assert.That(list.Select(a => (a.UserName, a.IsArchived)),
                Is.EqualTo(new[] { ("mara", false), ("tom", true) }));
            // The administrator role includes all others.
            Assert.That(list[0].Roles, Is.EqualTo(new[] { "Admin" }));
        }
    }

    [Test]
    public async Task RefusesWhatTheRulesForbid()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var scope = database.CreateIdentityScope();
        var (accounts, users) = await PrepareAsync(scope);
        var first = await accounts.CreateAsync(
            new NewAccount("mara", "Mara Quill", [Roles.Admin]));
        var mara = (await users.FindByNameAsync("mara"))!;

        var twice = await accounts.CreateAsync(
            new NewAccount("mara", "Someone else", [Roles.Reports]));
        var unknownRole = await accounts.CreateAsync(
            new NewAccount("ivo", "Ivo Stone", ["Driver"]));
        var samePassword = await accounts.ChangePasswordAsync(
            mara, first.Password!, first.Password!);
        var wrongCurrent = await accounts.ChangePasswordAsync(
            mara, "Not-The-Password-1", "Own-Choice-2026");
        var lastAdmin = await accounts.ArchiveAsync(mara.Id);
        var unknownUser = await accounts.ResetPasswordAsync("nobody");

        // A database without roles: adding the role throws after the
        // user was created.
        await using var empty = await TestDatabase.CreateAsync();
        await using var emptyScope = empty.CreateIdentityScope();
        var noRoles = emptyScope.ServiceProvider
            .GetRequiredService<UserAccountService>();
        var halfCreated =
            Assert.ThrowsAsync<InvalidOperationException>(
                () => noRoles.CreateAsync(
                new NewAccount("ivo", "Ivo Stone", [Roles.Reports])));
        await using var emptyDb = empty.CreateContext();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(twice.Errors, Is.EqualTo(new[]
                { "The user name 'mara' is already in use." }));
            Assert.That(unknownRole.Errors,
                Is.EqualTo(new[] { "Unknown role: Driver." }));
            Assert.That(samePassword.Errors.Single(), Is.EqualTo(
                "The new password must differ from the old one."));
            Assert.That(wrongCurrent.Errors, Is.EqualTo(new[]
                { "The current password is not correct." }));
            Assert.That(lastAdmin.Errors, Is.EqualTo(new[]
                { "The last administrator cannot be archived." }));
            Assert.That(unknownUser.Succeeded, Is.False);
            Assert.That(
                await users.Users.CountAsync(), Is.EqualTo(1));
            Assert.That(halfCreated, Is.Not.Null);
            Assert.That(await emptyDb.Users.AnyAsync(), Is.False);
        }
    }

    private static async Task<(UserAccountService,
        UserManager<ApplicationUser>)> PrepareAsync(
        AsyncServiceScope scope)
    {
        var services = scope.ServiceProvider;
        await services.GetRequiredService<AccountSetup>()
            .EnsureRolesAsync();
        return (services.GetRequiredService<UserAccountService>(),
            services
                .GetRequiredService<UserManager<ApplicationUser>>());
    }
}
