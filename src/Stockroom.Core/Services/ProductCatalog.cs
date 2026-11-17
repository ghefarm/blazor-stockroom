using Stockroom.Core.Entities;
using Stockroom.Core.Sample;

namespace Stockroom.Core.Services;

// The methods are async already, so the pages do not change when
// the products come from a database.
public class ProductCatalog
{
    public Task<IReadOnlyList<Product>> GetAllAsync() =>
        Task.FromResult(SampleProducts.All);

    public Task<Product?> FindByIdAsync(int id) =>
        Task.FromResult(SampleProducts.FindById(id));

    public Task<bool> ChangeStockQuantityAsync(
        int id, int quantity)
    {
        var found = SampleProducts.SetStockQuantity(id, quantity);
        return Task.FromResult(found);
    }
}
