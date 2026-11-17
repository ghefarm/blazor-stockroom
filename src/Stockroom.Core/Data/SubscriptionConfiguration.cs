using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Core.Entities;

namespace Stockroom.Core.Data;

public class SubscriptionConfiguration
    : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> row)
    {
        // One row with a known key, never a generated one.
        row.Property(s => s.Id).ValueGeneratedNever();
    }
}
