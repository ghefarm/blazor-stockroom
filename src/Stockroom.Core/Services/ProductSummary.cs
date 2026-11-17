using System.ComponentModel;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

// Sealed and immutable: HybridCache may then hand the same
// instance to every caller instead of a new copy per call.
[ImmutableObject(true)]
public sealed record ProductSummary(int Count, long StockValueCents)
{
    public static ProductSummary Create(IEnumerable<Product> products)
    {
        var list = products.ToList();

        return new ProductSummary(
            list.Count,
            list.Sum(p => p.PriceCents * p.StockQuantity));
    }
}
