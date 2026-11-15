using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Services;

// Every page that reads or changes suppliers comes through here, so
// a rule about suppliers is written once and can be tested alone.
public class SupplierDirectory(
    IDbContextFactory<StockroomDbContext> factory)
{
    public async Task<IReadOnlyList<Supplier>> GetAllAsync(
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<Supplier?> FindByIdAsync(
        int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public Task<SupplierChangeResult> AddAsync(
        Supplier supplier, CancellationToken ct = default) =>
        AddAllAsync([supplier], ct);

    public async Task<SupplierChangeResult> AddAllAsync(
        IReadOnlyList<Supplier> suppliers,
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        string[] names = [.. suppliers.Select(s => s.Name)];

        // People choose a supplier by name, so a name must be unique.
        // The unique index on the name is the safety net below this.
        if (names.Distinct().Count() < names.Length
            || await db.Suppliers.AnyAsync(
                s => names.Contains(s.Name), ct))
        {
            return SupplierChangeResult.NameTaken;
        }

        // One save, so a batch is stored completely or not at all.
        db.Suppliers.AddRange(suppliers);
        await db.SaveChangesAsync(ct);
        return SupplierChangeResult.Done;
    }

    public async Task<SupplierChangeResult> UpdateAsync(
        Supplier supplier, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.Suppliers.AnyAsync(
            s => s.Name == supplier.Name && s.Id != supplier.Id, ct))
        {
            return SupplierChangeResult.NameTaken;
        }

        var stored = await db.Suppliers
            .FirstOrDefaultAsync(s => s.Id == supplier.Id, ct);
        if (stored is null)
        {
            return SupplierChangeResult.NotFound;
        }

        stored.Name = supplier.Name;
        stored.Email = supplier.Email;
        stored.IsActive = supplier.IsActive;
        await db.SaveChangesAsync(ct);
        return SupplierChangeResult.Done;
    }

    public async Task<SupplierChangeResult> DeleteAsync(
        int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, ct);
        if (supplier is null)
        {
            return SupplierChangeResult.NotFound;
        }

        // Restrict would refuse the delete anyway. Asking first gives
        // the page a reason it can show instead of an exception.
        if (await db.PurchaseOrders.AnyAsync(
            o => o.SupplierId == id, ct))
        {
            return SupplierChangeResult.HasOrders;
        }

        db.Suppliers.Remove(supplier);
        await db.SaveChangesAsync(ct);
        return SupplierChangeResult.Done;
    }

    public async Task<int> DeactivateAsync(
        IReadOnlyCollection<int> ids, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        // One query and one save for all ticked rows.
        var active = await db.Suppliers
            .Where(s => ids.Contains(s.Id) && s.IsActive)
            .ToListAsync(ct);
        foreach (var supplier in active)
        {
            supplier.IsActive = false;
        }

        await db.SaveChangesAsync(ct);
        return active.Count;
    }
}
