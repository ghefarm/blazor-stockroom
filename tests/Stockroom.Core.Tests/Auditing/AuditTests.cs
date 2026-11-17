using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Stockroom.Core.Auditing;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Auditing;

public class AuditTests
{
    private static readonly DateTimeOffset Monday =
        new(2026, 7, 13, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Tuesday =
        new(2026, 7, 14, 15, 30, 0, TimeSpan.Zero);

    [Test]
    public async Task StampsTheCreatorAndTheLastChange()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.User.GetName().Returns("anna", "ben");
        database.Time.GetUtcNow().Returns(Monday, Tuesday);
        var catalog = database.CreateCatalog();

        var id = await AddProductAsync(database);
        await catalog.ChangePriceAsync(id, 500, 0);
        var product = await catalog.FindByIdAsync(id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(product!.CreatedBy, Is.EqualTo("anna"));
            Assert.That(product.CreatedAt,
                Is.EqualTo(Monday.UtcDateTime));
            Assert.That(product.LastModifiedBy, Is.EqualTo("ben"));
            Assert.That(product.LastModifiedAt,
                Is.EqualTo(Tuesday.UtcDateTime));
        }
    }

    [Test]
    public async Task KeepsTheCreatorWhateverTheWayOfSaving()
    {
        await using var database = await TestDatabase.CreateAsync();
        database.User.GetName().Returns("anna");
        database.Time.GetUtcNow().Returns(Monday);
        var id = await AddProductAsync(database);

        // Three more ways to save, by another user.
        database.User.GetName().Returns("ben");
        await using (var db = database.CreateContext())
        {
            // A detached copy, as a page would send it back.
            var copy = new Product
            {
                Id = id, Sku = "AU-01", Name = "Renamed",
            };
            db.Products.Update(copy);
            db.SaveChanges();

            db.Suppliers.Add(new() { Name = "Sync Supplies" });
            db.SaveChanges(acceptAllChangesOnSuccess: true);
            db.Suppliers.Add(new() { Name = "Async Supplies" });
            await db.SaveChangesAsync(true, CancellationToken.None);
        }

        await using var check = database.CreateContext();
        var product = await check.Products
            .SingleAsync(p => p.Id == id);
        var creators = await check.Suppliers
            .Select(s => s.CreatedBy).ToListAsync();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(product.CreatedBy, Is.EqualTo("anna"));
            Assert.That(product.CreatedAt,
                Is.EqualTo(Monday.UtcDateTime));
            Assert.That(product.LastModifiedBy, Is.EqualTo("ben"));
            Assert.That(creators, Is.EqualTo(new[] { "ben", "ben" }));
            Assert.That(AuditUser.GetName(null),
                Is.EqualTo("system"));
            Assert.That(AuditUser.GetName(anonymous),
                Is.EqualTo("system"));
        }
    }

    private static async Task<int> AddProductAsync(
        TestDatabase database)
    {
        await using var db = database.CreateContext();
        var product = new Product { Sku = "AU-01", Name = "Audit" };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product.Id;
    }
}
