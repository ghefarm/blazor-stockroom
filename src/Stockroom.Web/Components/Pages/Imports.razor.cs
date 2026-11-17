using Microsoft.AspNetCore.Components;
using Stockroom.Core;
using Stockroom.Core.Entities;

namespace Stockroom.Web.Components.Pages;

public partial class Imports
{
    [SupplyParameterFromForm(FormName = "price-list")]
    private IFormFile? PriceList { get; set; }

    [SupplyParameterFromQuery]
    private int? Changed { get; set; }

    [SupplyParameterFromQuery]
    private int? Skipped { get; set; }

    private IReadOnlyList<string> errors = [];
    private IReadOnlyList<PriceListImport> history = [];

    protected override async Task OnInitializedAsync()
    {
        history = await Importer.ListAsync();
    }

    private async Task ImportAsync()
    {
        if (PriceList is null || PriceList.Length == 0)
        {
            errors = ["Choose a file first."];
            return;
        }

        await using var stream = PriceList.OpenReadStream();
        var result = await Importer.ImportAsync(
            PriceList.FileName, stream);
        if (!result.Succeeded)
        {
            errors = result.Errors;
            return;
        }

        // Post/Redirect/Get: a reload does not post the file again.
        Navigation.NavigateTo(
            $"imports?changed={result.Prices!.Changed}"
            + $"&skipped={result.Prices.Skipped}");
    }

    private string FormatMoment(DateTime utc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, Clock.Zone);
        var day = DateOnly.FromDateTime(local);
        return BusinessFormats.FormatDate(day)
            + local.ToString(" HH:mm", BusinessFormats.Culture);
    }
}
