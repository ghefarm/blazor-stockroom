using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class DeliveryConfiguration
    : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> delivery)
    {
        delivery.Property(d => d.DeliveryNote).HasMaxLength(500);

        // A supplier that has deliveries must not disappear.
        delivery.HasOne<Supplier>()
            .WithMany()
            .HasForeignKey(d => d.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
