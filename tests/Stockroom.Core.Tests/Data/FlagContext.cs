using Microsoft.EntityFrameworkCore;

namespace Stockroom.Core.Tests.Data;

// A flag stored as a number, the way my real application did it.
public class Shelf
{
    public int Id { get; set; }
    public int Active { get; set; }
    public int Visible { get; set; } = -1;
}

public class FlagContext(DbContextOptions<FlagContext> options)
    : DbContext(options)
{
    public DbSet<Shelf> Shelves => Set<Shelf>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var shelf = modelBuilder.Entity<Shelf>();
        shelf.Property(s => s.Active).HasDefaultValue(1);
        shelf.Property(s => s.Visible)
            .HasDefaultValue(1)
            .HasSentinel(-1);
    }
}
