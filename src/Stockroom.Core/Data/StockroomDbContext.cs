using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class StockroomDbContext(
    DbContextOptions<StockroomDbContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders =>
        Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines =>
        Set<PurchaseOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Every IEntityTypeConfiguration class in this project.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StockroomDbContext).Assembly);
    }
}
