using System.ComponentModel.DataAnnotations;

namespace Stockroom.Web.Components.Products;

// What the stock count form posts. The stepper cannot go below 0,
// but a hand-made POST can, so the rule lives here as well.
public class StockCountInput
{
    [Range(0, 100_000)]
    public int CountedQuantity { get; set; }
}
