using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class SupplierDirectoryTests
{
    [Test]
    public async Task AddsChangesDeactivatesAndDeletes()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var directory =
            new SupplierDirectory(database.CreateFactory());
        var before = await directory.GetAllAsync();
        var kestrel =
            before.Single(s => s.Name == "Kestrel Lifting Gear");
        var oldMill = before.Single(s => s.Name == "Old Mill Labels");

        var added = await directory.AddAllAsync(
        [
            new() { Name = "North Pallets", Email = "a@north.test" },
            new() { Name = "Harbor Tools", Email = "b@harbor.test" },
        ]);
        kestrel.Email = "orders@kestrel.example";
        var updated = await directory.UpdateAsync(kestrel);
        var deactivated =
            await directory.DeactivateAsync([kestrel.Id]);
        var deleted = await directory.DeleteAsync(oldMill.Id);
        var after = await directory.GetAllAsync();
        var changed = after.Single(s => s.Id == kestrel.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(added, Is.EqualTo(SupplierChangeResult.Done));
            Assert.That(updated,
                Is.EqualTo(SupplierChangeResult.Done));
            Assert.That(deactivated, Is.EqualTo(1));
            Assert.That(deleted,
                Is.EqualTo(SupplierChangeResult.Done));
            Assert.That(after, Has.Count.EqualTo(5));
            Assert.That(changed.Email,
                Is.EqualTo("orders@kestrel.example"));
            Assert.That(changed.IsActive, Is.False);
        }
    }

    [Test]
    public async Task RefusesTakenNamesUnknownRowsAndSuppliersInUse()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var directory =
            new SupplierDirectory(database.CreateFactory());
        var before = await directory.GetAllAsync();
        var brightway =
            before.Single(s => s.Name == "Brightway Packaging");
        var meadow =
            before.Single(s => s.Name == "Meadow Safety Supplies");

        var taken = await directory.AddAsync(new()
        {
            Name = "Brightway Packaging",
            Email = "x@brightway.example",
        });
        var twice = await directory.AddAllAsync(
        [
            new() { Name = "Twin Crates", Email = "a@twin.example" },
            new() { Name = "Twin Crates", Email = "b@twin.example" },
        ]);
        var renamed = await directory.UpdateAsync(new()
        {
            Id = meadow.Id,
            Name = "Kestrel Lifting Gear",
            Email = meadow.Email,
        });
        var unknown = await directory.UpdateAsync(
            new() { Id = 999, Name = "Nobody" });
        var inUse = await directory.DeleteAsync(brightway.Id);
        var missing = await directory.DeleteAsync(999);
        var after = await directory.GetAllAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(new[] { taken, twice, renamed },
                Is.All.EqualTo(SupplierChangeResult.NameTaken));
            Assert.That(new[] { unknown, missing },
                Is.All.EqualTo(SupplierChangeResult.NotFound));
            Assert.That(inUse,
                Is.EqualTo(SupplierChangeResult.HasOrders));
            Assert.That(after.Select(s => s.Name),
                Is.EqualTo(before.Select(s => s.Name)));
        }
    }
}
