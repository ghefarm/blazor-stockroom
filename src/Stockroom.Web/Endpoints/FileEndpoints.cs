using Microsoft.AspNetCore.Authorization;
using Stockroom.Core;
using Stockroom.Core.Exports;
using Stockroom.Core.Services;

namespace Stockroom.Web.Endpoints;

public static class FileEndpoints
{
    private const string ExcelType = "application/vnd.openxmlformats-"
        + "officedocument.spreadsheetml.sheet";

    public static void MapFileEndpoints(
        this IEndpointRouteBuilder app)
    {
        // The same roles as the page /imports that links here.
        var allowed = new AuthorizeAttribute
        {
            Roles = Roles.PurchasingOrAdmin,
        };
        var exports = app.MapGroup("/exports")
            .RequireAuthorization(allowed);
        exports.MapGet("/products.csv", ExportCsvAsync);
        exports.MapGet("/products.xlsx", ExportExcelAsync);
        exports.MapGet("/products.pdf", ExportPdfAsync);
        exports.MapGet("/price-lists.zip", ExportZipAsync);
        app.MapGet("/imports/{id:int}/file", DownloadImportAsync)
            .RequireAuthorization(allowed);
    }

    private static async Task<IResult> ExportCsvAsync(
        ProductCatalog catalog, CancellationToken ct)
    {
        var products = await catalog.GetAllAsync(ct);
        var bytes = ProductCsvExport.Create(products);
        return Results.File(bytes, "text/csv", "products.csv");
    }

    private static async Task<IResult> ExportExcelAsync(
        ProductCatalog catalog, HttpRequest request,
        CancellationToken ct)
    {
        var products = await catalog.GetAllAsync(ct);

        // The address the user typed, so the links in the file
        // lead back to the same server.
        var site = new Uri(
            $"{request.Scheme}://{request.Host}{request.PathBase}/");
        var bytes = ProductExcelExport.Create(products, site);
        return Results.File(bytes, ExcelType, "products.xlsx");
    }

    private static async Task<IResult> ExportPdfAsync(
        ProductCatalog catalog, BusinessClock clock,
        CancellationToken ct)
    {
        var products = await catalog.GetAllAsync(ct);
        var bytes = ProductPdfExport.Create(products, clock.Today);

        // No download name: the browser shows the PDF in a tab.
        return Results.File(bytes, "application/pdf");
    }

    private static async Task<IResult> ExportZipAsync(
        PriceListImporter importer, ImportArchive archive,
        BusinessClock clock, HttpContext context,
        CancellationToken ct)
    {
        var imports = await importer.ListAsync(ct);

        // A temporary file, not memory: the ZIP can be large.
        var path = Path.Combine(
            Path.GetTempPath(), Path.GetRandomFileName());
        var zip = new FileStream(path, FileMode.CreateNew,
            FileAccess.ReadWrite, FileShare.None, 81_920,
            FileOptions.DeleteOnClose | FileOptions.Asynchronous);

        // Closed, and so deleted, when the request ends, also when
        // the browser leaves early or an exception is thrown.
        context.Response.RegisterForDisposeAsync(zip);

        ImportZipExport.Write(
            zip, imports, archive.Folder, clock.Zone);
        zip.Position = 0;
        return Results.File(
            zip, "application/zip", "price-lists.zip");
    }

    private static async Task<IResult> DownloadImportAsync(
        int id, PriceListImporter importer, CancellationToken ct)
    {
        var path = await importer.FindArchivedFileAsync(id, ct);
        if (path is null)
        {
            return Results.NotFound();
        }

        return TypedResults.PhysicalFile(
            path, "text/csv", Path.GetFileName(path));
    }
}
