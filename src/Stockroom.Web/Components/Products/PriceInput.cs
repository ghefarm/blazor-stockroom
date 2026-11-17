using System.ComponentModel.DataAnnotations;
using Stockroom.Core;
using Stockroom.Core.Entities;

namespace Stockroom.Web.Components.Products;

// What the price form posts. The price stays text, so Money reads
// it the same way as an imported price list. The version is the
// one the user saw when the form opened.
public class PriceInput : IValidatableObject
{
    [Required]
    public string? Price { get; set; }

    public int Version { get; set; }

    // Zero for text that Money cannot read. Validation refuses it.
    public long Cents =>
        Money.TryParse(Price, out var cents) ? cents : 0;

    public static PriceInput FromProduct(Product product) => new()
    {
        Price = Money.FormatForInput(product.PriceCents),
        Version = product.Version,
    };

    public IEnumerable<ValidationResult> Validate(
        ValidationContext context)
    {
        if (Cents <= 0)
        {
            yield return new ValidationResult(
                "Enter a price above zero, such as 12.50.",
                [nameof(Price)]);
        }
    }
}
