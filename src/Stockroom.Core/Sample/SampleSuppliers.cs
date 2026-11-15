using Stockroom.Core.Entities;

namespace Stockroom.Core.Sample;

// In-memory store until Stockroom has a database. It lives as long
// as the app runs, and a restart brings back the four suppliers.
public static class SampleSuppliers
{
    // Requests run on several threads at the same time, so every
    // access to the list goes through this lock.
    private static readonly Lock gate = new();

    private static readonly List<Supplier> suppliers =
    [
        new() { Id = 1, Name = "Brightway Packaging",
            Email = "orders@brightway.example", IsActive = true },
        new() { Id = 2, Name = "Kestrel Lifting Gear",
            Email = "sales@kestrel.example", IsActive = true },
        new() { Id = 3, Name = "Meadow Safety Supplies",
            Email = "hello@meadow-safety.example", IsActive = true },
        new() { Id = 4, Name = "Old Mill Labels",
            Email = "office@oldmill.example", IsActive = false },
    ];

    private static int nextId = 5;

    public static IReadOnlyList<Supplier> ListAll()
    {
        lock (gate)
        {
            return [.. suppliers.OrderBy(s => s.Name).Select(Copy)];
        }
    }

    public static Supplier? FindById(int id)
    {
        lock (gate)
        {
            var supplier = suppliers.Find(s => s.Id == id);
            return supplier is null ? null : Copy(supplier);
        }
    }

    public static int Add(Supplier supplier)
    {
        lock (gate)
        {
            var stored = Copy(supplier);
            stored.Id = nextId++;
            suppliers.Add(stored);
            return stored.Id;
        }
    }

    public static bool Update(Supplier supplier)
    {
        lock (gate)
        {
            var index = suppliers.FindIndex(s => s.Id == supplier.Id);

            if (index < 0)
            {
                return false;
            }

            suppliers[index] = Copy(supplier);
            return true;
        }
    }

    public static bool Remove(int id)
    {
        lock (gate)
        {
            return suppliers.RemoveAll(s => s.Id == id) > 0;
        }
    }

    public static int Deactivate(IEnumerable<int> ids)
    {
        lock (gate)
        {
            var matching = suppliers.Where(s => ids.Contains(s.Id));
            var count = 0;

            foreach (var supplier in matching)
            {
                supplier.IsActive = false;
                count++;
            }

            return count;
        }
    }

    // Callers get copies, so a page cannot change a stored supplier
    // without calling Update.
    private static Supplier Copy(Supplier supplier) => new()
    {
        Id = supplier.Id,
        Name = supplier.Name,
        Email = supplier.Email,
        IsActive = supplier.IsActive,
    };
}
