using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Stockroom.Core.Data;
using Stockroom.Core.Entities;
using Stockroom.Core.Files;

namespace Stockroom.Core.Services;

public class PriceListImporter(
    IDbContextFactory<StockroomDbContext> factory,
    ImportArchive archive,
    BusinessClock clock,
    ProductSummaryService summaries)
{
    public const int MaxBytes = 2 * 1024 * 1024;

    public async Task<ImportResult> ImportAsync(
        string fileName, Stream content,
        CancellationToken ct = default)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".csv",
                StringComparison.OrdinalIgnoreCase))
        {
            return ImportResult.Fail(
                "Only .csv files can be imported.");
        }

        if (!CanWriteArchive())
        {
            return ImportResult.Fail(
                "The archive folder is not writable.");
        }

        var bytes = await ReadAllAsync(content, ct);
        if (bytes is null)
        {
            return ImportResult.Fail("The file is larger than 2 MB.");
        }

        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        var earlier = await FindByHashAsync(hash, ct);
        if (earlier is not null)
        {
            return ImportResult.Fail(
                $"This file was imported before as {earlier}.");
        }

        return await ImportCheckedAsync(fileName, bytes, hash, ct);
    }

    private async Task<ImportResult> ImportCheckedAsync(
        string fileName, byte[] bytes, string hash,
        CancellationToken ct)
    {
        var list = PriceListReader.Read(bytes);
        if (list.Errors.Count > 0)
        {
            return new ImportResult(null, list.Errors);
        }

        var path = await ArchiveAsync(fileName, bytes, ct);
        try
        {
            var prices = await SaveAsync(
                list.Changes, Path.GetFileName(path), hash, ct);
            return new ImportResult(prices, []);
        }
        catch
        {
            // Without its database row, the copy would be an orphan.
            File.Delete(path);
            throw;
        }
    }

    private bool CanWriteArchive()
    {
        try
        {
            Directory.CreateDirectory(archive.Folder);
            var probe = Path.Combine(
                archive.Folder, $"probe-{Guid.NewGuid():N}.tmp");
            File.WriteAllBytes(probe, []);
            File.Delete(probe);
            return true;
        }
        catch (Exception e) when (
            e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static async Task<byte[]?> ReadAllAsync(
        Stream content, CancellationToken ct)
    {
        using var memory = new MemoryStream();
        var buffer = new byte[81_920];
        int read;
        while ((read = await content.ReadAsync(buffer, ct)) > 0)
        {
            // Stop early, so a huge file never fills the memory.
            if (memory.Length + read > MaxBytes)
            {
                return null;
            }

            memory.Write(buffer, 0, read);
        }

        return memory.ToArray();
    }

    private async Task<string?> FindByHashAsync(
        string hash, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.PriceListImports
            .Where(i => i.FileHash == hash)
            .Select(i => i.FileName)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<string> ArchiveAsync(
        string fileName, byte[] bytes, CancellationToken ct)
    {
        var name = FileNames.MakeSafe(fileName, "price-list.csv");
        var stem = Path.GetFileNameWithoutExtension(name);
        var path = Path.Combine(archive.Folder, name);
        for (var n = 2; File.Exists(path); n++)
        {
            path = Path.Combine(archive.Folder, $"{stem} ({n}).csv");
        }

        if (!SafePath.IsInside(archive.Folder, path))
        {
            throw new InvalidOperationException(
                $"'{fileName}' leads out of the archive folder.");
        }

        // CreateNew never overwrites a file of another import.
        await using var file = new FileStream(
            path, FileMode.CreateNew, FileAccess.Write);
        await file.WriteAsync(bytes, ct);
        return path;
    }

    private async Task<BatchResult> SaveAsync(
        IReadOnlyList<PriceChange> changes, string fileName,
        string hash, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var prices = await ProductCatalog.ApplyPricesAsync(
            db, changes, ct);

        db.PriceListImports.Add(new PriceListImport
        {
            FileName = fileName,
            FileHash = hash,
            ImportedAt = clock.UtcNow.UtcDateTime,
            RowCount = changes.Count,
        });

        // One save is one transaction: prices and history together.
        await db.SaveChangesAsync(ct);
        await summaries.InvalidateAsync();
        return prices;
    }

    public async Task<IReadOnlyList<PriceListImport>> ListAsync(
        CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.PriceListImports
            .AsNoTracking()
            .OrderByDescending(i => i.Id)
            .ToListAsync(ct);
    }

    public async Task<string?> FindArchivedFileAsync(
        int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var name = await db.PriceListImports
            .Where(i => i.Id == id)
            .Select(i => i.FileName)
            .FirstOrDefaultAsync(ct);
        if (name is null)
        {
            return null;
        }

        // Checked again: the row may have been changed by hand.
        var path = Path.Combine(archive.Folder, name);
        if (!SafePath.IsInside(archive.Folder, path)
            || !File.Exists(path))
        {
            return null;
        }

        return path;
    }
}
