using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class MatchingSettingsConfiguration
    : IEntityTypeConfiguration<MatchingSettings>
{
    public void Configure(EntityTypeBuilder<MatchingSettings> settings)
    {
        // Every installation needs the one row, not only a database
        // with sample data, so the migration inserts it.
        settings.HasData(new MatchingSettings());
    }
}
