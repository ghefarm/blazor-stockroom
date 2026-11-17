using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class DeliveryMatchConfiguration
    : IEntityTypeConfiguration<DeliveryMatch>
{
    public void Configure(EntityTypeBuilder<DeliveryMatch> match)
    {
        // A match row means nothing without its delivery.
        match.HasOne<Delivery>()
            .WithMany()
            .HasForeignKey(m => m.DeliveryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Confirmed and rejected rows are the memory of the engine,
        // so an order with match rows must not disappear.
        match.HasOne<PurchaseOrder>()
            .WithMany()
            .HasForeignKey(m => m.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
