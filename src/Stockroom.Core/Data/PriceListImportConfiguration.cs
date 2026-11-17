using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class PriceListImportConfiguration
    : IEntityTypeConfiguration<PriceListImport>
{
    public void Configure(EntityTypeBuilder<PriceListImport> import)
    {
        import.Property(i => i.FileName).HasMaxLength(260);
        import.Property(i => i.FileHash).HasMaxLength(64);

        // The safety net: one file content, one import.
        import.HasIndex(i => i.FileHash).IsUnique();
    }
}
