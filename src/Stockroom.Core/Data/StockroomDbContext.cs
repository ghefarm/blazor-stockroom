using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class StockroomDbContext(
    DbContextOptions<StockroomDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders =>
        Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines =>
        Set<PurchaseOrderLine>();
    public DbSet<PriceListImport> PriceListImports =>
        Set<PriceListImport>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryMatch> DeliveryMatches =>
        Set<DeliveryMatch>();
    public DbSet<MatchingSettings> MatchingSettings =>
        Set<MatchingSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The Identity tables. Their schema version comes from the
        // Identity options of the application service provider.
        base.OnModelCreating(modelBuilder);

        // Every IEntityTypeConfiguration class in this project.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(StockroomDbContext).Assembly);
    }
}
