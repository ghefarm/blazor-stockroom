namespace Stockroom.Core.Services;

public sealed record StockCheckResult(
    int Checked, int Low, int OutOfStock)
{
    public string Describe()
    {
        var products = Checked == 1 ? "product" : "products";
        return $"Checked {Checked} {products}: "
            + $"{Low} low, {OutOfStock} out of stock.";
    }
}
