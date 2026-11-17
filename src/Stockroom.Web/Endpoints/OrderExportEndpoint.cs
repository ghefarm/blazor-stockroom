using Microsoft.AspNetCore.Authorization;
using Stockroom.Core;
using Stockroom.Core.Files;
using Stockroom.Core.Services;

namespace Stockroom.Web.Endpoints;

public static class OrderExportEndpoint
{
    public static void MapOrderExport(this IEndpointRouteBuilder app)
    {
        // The same roles as the page /orders that links here.
        app.MapGet("/orders/export", ExportAsync)
            .RequireAuthorization(new AuthorizeAttribute
            {
                Roles = Roles.PurchasingOrAdmin,
            });
    }

    private static async Task<IResult> ExportAsync(
        string? supplier, string? status, string? from,
        string? to, string? q,
        OrderListService orderList, CancellationToken ct)
    {
        // A file shows no filter fields. A value that the page would
        // quietly leave out must stop the export instead.
        if (!OrderListQuery.TryRead(
            supplier, status, from, to, q, out var filter))
        {
            return Results.BadRequest("The filter is not valid.");
        }

        var rows = await orderList.ExportAsync(filter, ct);
        var csv = CsvText.EncodeWithBom(OrderExport.CreateCsv(rows));
        return Results.File(
            csv, "text/csv", OrderExport.CreateFileName(filter));
    }
}
