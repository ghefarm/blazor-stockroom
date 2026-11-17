using Stockroom.Core.Services;

namespace Stockroom.Core.Tests.Services;

public class ProductConcurrencyTests
{
    [Test]
    public async Task SavesWhenNobodyChangedTheProductInBetween()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var catalog = database.CreateCatalog();

        var opened = await catalog.FindByIdAsync(1);
        var first = await catalog.ChangePriceAsync(
            1, 9500, opened!.Version);
        var reopened = await catalog.FindByIdAsync(1);
        var second = await catalog.ChangePriceAsync(
            1, 9900, reopened!.Version);
        var now = await catalog.FindByIdAsync(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first, Is.EqualTo(SaveOutcome.Saved));
            Assert.That(second, Is.EqualTo(SaveOutcome.Saved));
            Assert.That(now!.PriceCents, Is.EqualTo(9900));
            Assert.That(now.Version, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task RefusesASaveBasedOnOldValues()
    {
        await using var database = await TestDatabase.CreateAsync();
        await database.SeedAsync();
        var catalog = database.CreateCatalog();

        // Ana and Ben open the same shelf: both see 89.00.
        var ana = await catalog.FindByIdAsync(1);
        var ben = await catalog.FindByIdAsync(1);
        var anaSaved = await catalog.ChangePriceAsync(
            1, 9500, ana!.Version);
        var benSaved = await catalog.ChangePriceAsync(
            1, 9900, ben!.Version);

        // A batch write without the change tracker counts too.
        var gloves = await catalog.FindByIdAsync(6);
        await catalog.DeactivateAsync(["GL-M"]);
        var glovesSaved = await catalog.ChangePriceAsync(
            6, 500, gloves!.Version);

        var unknown = await catalog.ChangePriceAsync(999, 100, 0);
        var shelf = await catalog.FindByIdAsync(1);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(anaSaved, Is.EqualTo(SaveOutcome.Saved));
            Assert.That(benSaved, Is.EqualTo(SaveOutcome.Conflict));
            Assert.That(glovesSaved,
                Is.EqualTo(SaveOutcome.Conflict));
            Assert.That(unknown, Is.EqualTo(SaveOutcome.NotFound));
            Assert.That(shelf!.PriceCents, Is.EqualTo(9500));
        }
    }
}
