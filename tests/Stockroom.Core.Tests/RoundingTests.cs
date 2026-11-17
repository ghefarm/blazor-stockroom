namespace Stockroom.Core.Tests;

public class RoundingTests
{
    [Test]
    public void RoundedRows_AddUpToTheShownTotal()
    {
        long[] lines = [105, 105, 105];

        long[] discounts =
            [.. lines.Select(l => Money.CalculatePercent(l, 10m))];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(discounts,
                Is.EqualTo(new long[] { 11, 11, 11 }));
            Assert.That(discounts.Sum(), Is.EqualTo(33));
            Assert.That(Money.Split(1_000, 3),
                Is.EqualTo(new long[] { 334, 333, 333 }));
        }
    }

    [Test]
    public void Split_RefusesWhatItCannotShareFairly()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(() => Money.Split(1_000, 0),
                Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => Money.Split(-1, 2),
                Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
