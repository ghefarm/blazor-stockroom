using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class SupplierConfiguration
    : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> supplier)
    {
        supplier.Property(s => s.Name).HasMaxLength(100);
        supplier.Property(s => s.Email).HasMaxLength(200);

        // People choose a supplier by name, so a name must be unique.
        supplier.HasIndex(s => s.Name).IsUnique();

        supplier.Property(s => s.IsActive).HasDefaultValue(true);
    }
}
