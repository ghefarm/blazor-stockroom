using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> product)
    {
        product.Property(p => p.Sku).HasMaxLength(20);
        product.Property(p => p.Name).HasMaxLength(100);

        // Labels and imports find a product by its SKU.
        product.HasIndex(p => p.Sku).IsUnique();

        // For rows that EF Core does not write: SQL scripts, and
        // existing rows when a later migration adds a column.
        product.Property(p => p.IsActive).HasDefaultValue(true);

        // Part of the WHERE clause of every UPDATE and DELETE.
        product.Property(p => p.Version).IsConcurrencyToken();
    }
}
