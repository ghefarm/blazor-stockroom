namespace Stockroom.Core.Entities;

public class PriceListImport
{
    public int Id { get; set; }

    // The name in the archive folder, not the name on the
    // user's disk.
    public string FileName { get; set; } = "";

    // SHA-256 of the file content, as 64 hex characters.
    public string FileHash { get; set; } = "";

    // UTC, see chapter 15.
    public DateTime ImportedAt { get; set; }

    public int RowCount { get; set; }
}
