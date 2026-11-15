using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class PurchaseOrderConfiguration
    : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> order)
    {
        order.Property(o => o.OrderNumber).HasMaxLength(20);
        order.HasIndex(o => o.OrderNumber).IsUnique();

        // The order list filters and sorts by this date.
        order.HasIndex(o => o.OrderedOn);

        // A supplier that has orders must not disappear.
        order.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(o => o.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lines belong to their order and are deleted with it.
        order.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey(l => l.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
