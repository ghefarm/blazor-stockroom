using Stockroom.Core.Entities;
using Stockroom.Core.Sample;

namespace Stockroom.Core.Services;

// Every page that reads or changes suppliers comes through here, so
// a rule about suppliers is written once and can be tested alone.
public class SupplierDirectory
{
    public Task<IReadOnlyList<Supplier>> GetAllAsync() =>
        Task.FromResult(SampleSuppliers.ListAll());

    public Task<Supplier?> FindByIdAsync(int id) =>
        Task.FromResult(SampleSuppliers.FindById(id));

    public Task<SupplierChangeResult> AddAsync(Supplier supplier) =>
        AddAllAsync([supplier]);

    public Task<SupplierChangeResult> AddAllAsync(
        IReadOnlyList<Supplier> suppliers)
    {
        // People choose a supplier by name, so a name must be unique.
        // A batch with one taken name adds nothing at all.
        var names = suppliers.Select(s => s.Name).ToList();
        if (names.Distinct().Count() < names.Count
            || names.Any(name => IsNameTaken(name, exceptId: 0)))
        {
            return Task.FromResult(SupplierChangeResult.NameTaken);
        }

        foreach (var supplier in suppliers)
        {
            SampleSuppliers.Add(supplier);
        }

        return Task.FromResult(SupplierChangeResult.Done);
    }

    public Task<SupplierChangeResult> UpdateAsync(Supplier supplier)
    {
        if (IsNameTaken(supplier.Name, exceptId: supplier.Id))
        {
            return Task.FromResult(SupplierChangeResult.NameTaken);
        }

        var found = SampleSuppliers.Update(supplier);
        return Task.FromResult(found
            ? SupplierChangeResult.Done
            : SupplierChangeResult.NotFound);
    }

    public Task<SupplierChangeResult> DeleteAsync(int id)
    {
        var found = SampleSuppliers.Remove(id);
        return Task.FromResult(found
            ? SupplierChangeResult.Done
            : SupplierChangeResult.NotFound);
    }

    public Task<int> DeactivateAsync(IReadOnlyCollection<int> ids) =>
        Task.FromResult(SampleSuppliers.Deactivate(ids));

    private static bool IsNameTaken(string name, int exceptId) =>
        SampleSuppliers.ListAll()
            .Any(s => s.Name == name && s.Id != exceptId);
}
