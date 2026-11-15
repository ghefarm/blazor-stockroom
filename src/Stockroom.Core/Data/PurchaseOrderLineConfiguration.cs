using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class PurchaseOrderLineConfiguration
    : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> line)
    {
        // A product that was ordered must not disappear.
        line.HasOne<Product>()
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
