namespace Stockroom.Web.Components.Suppliers;

// The root of the batch form. The attribute lets the validation
// source generator walk into every row of the list.
[ValidatableType]
public class SupplierBatchInput
{
    public List<SupplierInput> Rows { get; set; } = [];

    public static SupplierBatchInput Create(int rowCount) => new()
    {
        Rows = [.. Enumerable.Range(0, rowCount)
            .Select(_ => new SupplierInput())],
    };
}
