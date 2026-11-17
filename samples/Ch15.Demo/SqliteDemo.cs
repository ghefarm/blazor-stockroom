using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ch15.Demo;

public class PriceRow
{
    public int Id { get; set; }

    public decimal Price { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public DateTime ChangedAtUtc { get; set; }
}

public class DemoDbContext(SqliteConnection connection) : DbContext
{
    public DbSet<PriceRow> Prices => Set<PriceRow>();

    protected override void OnConfiguring(
        DbContextOptionsBuilder options)
    {
        options.UseSqlite(connection);
    }
}

public static class SqliteDemo
{
    public static void Run()
    {
        Console.WriteLine("--- decimal on SQLite ---");

        using var connection = new SqliteConnection(
            "Data Source=:memory:");
        connection.Open();
        AddRows(connection);

        using var db = new DemoDbContext(connection);
        ShowEfCoreQueries(db);
        ShowRawSql(connection);
        ShowDateTimeOffsetOrdering(db);
        ShowUtcDateTimeOrdering(db);
    }

    private static void AddRows(SqliteConnection connection)
    {
        // In real time row 3 comes first and row 1 comes last.
        var chicago = TimeSpan.FromHours(-6);
        var utc = TimeSpan.Zero;
        using var db = new DemoDbContext(connection);
        db.Database.EnsureCreated();
        db.Prices.AddRange(
            CreateRow(9.50m, new(2026, 3, 1, 9, 0, 0, chicago)),
            CreateRow(10.00m, new(2026, 3, 1, 10, 0, 0, utc)),
            CreateRow(100.25m, new(2026, 3, 1, 8, 0, 0, utc)));
        db.SaveChanges();
    }

    private static PriceRow CreateRow(
        decimal price, DateTimeOffset at)
    {
        return new PriceRow
        {
            Price = price,
            ChangedAt = at,
            ChangedAtUtc = at.UtcDateTime,
        };
    }

    private static void ShowEfCoreQueries(DemoDbContext db)
    {
        var ordered = db.Prices.OrderBy(p => p.Price)
            .Select(p => p.Price);
        Console.WriteLine(ordered.ToQueryString());
        Console.WriteLine(string.Join(", ", ordered.ToList()));
        Console.WriteLine(db.Prices.Sum(p => p.Price));
        Console.WriteLine(db.Prices.Count(p => p.Price > 9.99m));
    }

    private static void ShowRawSql(SqliteConnection connection)
    {
        string[] queries =
        [
            "SELECT typeof(Price) FROM Prices LIMIT 1",
            "SELECT group_concat(Price, ', ') FROM "
                + "(SELECT Price FROM Prices ORDER BY Price)",
            "SELECT COUNT(*) FROM Prices WHERE Price > 9.99",
            "SELECT typeof(SUM(Price)) FROM Prices",
            "SELECT group_concat(Id, ', ') FROM "
                + "(SELECT Id FROM Prices ORDER BY ChangedAt)",
        ];
        foreach (var sql in queries)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            Console.WriteLine($"SQL: {command.ExecuteScalar()}");
        }
    }

    private static void ShowDateTimeOffsetOrdering(DemoDbContext db)
    {
        try
        {
            var first = db.Prices.OrderBy(p => p.ChangedAt).First();
            Console.WriteLine($"First by DateTimeOffset: {first.Id}");
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static void ShowUtcDateTimeOrdering(DemoDbContext db)
    {
        var rows = db.Prices.AsNoTracking()
            .OrderBy(p => p.ChangedAtUtc)
            .ToList();
        Console.WriteLine(string.Join(", ", rows.Select(r => r.Id)));
        Console.WriteLine(rows[0].ChangedAtUtc.Kind);
    }
}
