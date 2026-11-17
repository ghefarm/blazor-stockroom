using Stockroom.Web.Components.Products;

namespace Stockroom.Web.Tests.Components.Products;

// NUnit shares one instance of a class between its tests. A new
// instance per test gives every test its own bUnit context.
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class StockBadgeTests : BunitContext
{
    private const string Full =
        """<span class="badge text-bg-success">In stock</span>""";

    private const string Low =
        """<span class="badge text-bg-warning">Low stock</span>""";

    private const string Empty =
        """<span class="badge text-bg-danger">Out of stock</span>""";

    [Test]
    public void ShowsInStockForAFullShelf()
    {
        var cut = RenderBadge(50);

        cut.MarkupMatches(Full);
    }

    [Test]
    public void WarnsAboutLowAndEmptyShelves()
    {
        RenderBadge(9).MarkupMatches(Low);
        RenderBadge(0).MarkupMatches(Empty);
        RenderBadge(-3).MarkupMatches(Empty);
    }

    private IRenderedComponent<StockBadge> RenderBadge(
        int quantity) =>
        Render<StockBadge>(parameters => parameters
            .Add(p => p.Quantity, quantity));
}
