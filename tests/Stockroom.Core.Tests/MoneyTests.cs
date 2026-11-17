namespace Stockroom.Core.Tests;

public class MoneyTests
{
    [Test]
    public void TryParse_ReadsDotAndComma_AndFormatsBack()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Money.TryParse("1234.5", out var dot));
            Assert.That(dot, Is.EqualTo(123_450));
            Assert.That(Money.TryParse(" 1,5 ", out var comma));
            Assert.That(comma, Is.EqualTo(150));
            Assert.That(Money.Format(123_450),
                Is.EqualTo("1,234.50"));
            Assert.That(Money.FormatForInput(123_450),
                Is.EqualTo("1234.50"));
        }
    }

    [TestCase(null)]
    [TestCase("  ")]
    [TestCase("ten")]
    [TestCase("1,234.50")]
    [TestCase("1.234")]
    [TestCase("12 345")]
    [TestCase("1e3")]
    [TestCase("79228162514264337593543950335")]
    public void TryParse_RefusesUnclearInput(string? text)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Money.TryParse(text, out var cents),
                Is.False);
            Assert.That(cents, Is.Zero);
        }
    }
}
