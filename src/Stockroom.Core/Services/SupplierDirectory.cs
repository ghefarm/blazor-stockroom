using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

// Every page that reads or changes suppliers comes through here, so
// a rule about suppliers is written once and can be tested alone.
public class SupplierDirectory(
    IDbContextFactory<StockroomDbContext> factory)
{
    public async Task<IReadOnlyList<Supplier>> GetAllAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Suppliers
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Supplier?> FindByIdAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.Suppliers.FindAsync(id);
    }

    public Task<SupplierChangeResult> AddAsync(Supplier supplier) =>
        AddAllAsync([supplier]);

    public async Task<SupplierChangeResult> AddAllAsync(
        IReadOnlyList<Supplier> suppliers)
    {
        await using var db = await factory.CreateDbContextAsync();
        string[] names = [.. suppliers.Select(s => s.Name)];

        // People choose a supplier by name, so a name must be unique.
        // The unique index on the name is the safety net below this.
        if (names.Distinct().Count() < names.Length
            || await db.Suppliers.AnyAsync(
                s => names.Contains(s.Name)))
        {
            return SupplierChangeResult.NameTaken;
        }

        // One save, so a batch is stored completely or not at all.
        db.Suppliers.AddRange(suppliers);
        await db.SaveChangesAsync();
        return SupplierChangeResult.Done;
    }

    public async Task<SupplierChangeResult> UpdateAsync(
        Supplier supplier)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (await db.Suppliers.AnyAsync(
            s => s.Name == supplier.Name && s.Id != supplier.Id))
        {
            return SupplierChangeResult.NameTaken;
        }

        var stored = await db.Suppliers.FindAsync(supplier.Id);
        if (stored is null)
        {
            return SupplierChangeResult.NotFound;
        }

        stored.Name = supplier.Name;
        stored.Email = supplier.Email;
        stored.IsActive = supplier.IsActive;
        await db.SaveChangesAsync();
        return SupplierChangeResult.Done;
    }

    public async Task<SupplierChangeResult> DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var supplier = await db.Suppliers.FindAsync(id);
        if (supplier is null)
        {
            return SupplierChangeResult.NotFound;
        }

        db.Suppliers.Remove(supplier);
        await db.SaveChangesAsync();
        return SupplierChangeResult.Done;
    }

    public async Task<int> DeactivateAsync(IReadOnlyCollection<int> ids)
    {
        await using var db = await factory.CreateDbContextAsync();
        var active = await db.Suppliers
            .Where(s => ids.Contains(s.Id) && s.IsActive)
            .ToListAsync();
        foreach (var supplier in active)
        {
            supplier.IsActive = false;
        }

        await db.SaveChangesAsync();
        return active.Count;
    }
}
