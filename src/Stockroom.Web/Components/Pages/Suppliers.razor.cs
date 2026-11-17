using Microsoft.AspNetCore.Components;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;
using Stockroom.Web.Components.Shared;
using Stockroom.Web.Components.Suppliers;

namespace Stockroom.Web.Components.Pages;

// Only the length moved this code out of the .razor file. The page
// holds no rules, so this class is for reading, not for testing.
public partial class Suppliers(
    SupplierDirectory directory, NavigationManager navigation)
{
    private IReadOnlyList<Supplier> suppliers = [];
    private Supplier? toDelete;
    private Notice? notice;
    private int activeCount;

    [SupplyParameterFromQuery(Name = "done")]
    private string? Done { get; set; }

    [SupplyParameterFromQuery(Name = "delete")]
    private int? DeleteId { get; set; }

    [SupplyParameterFromForm(FormName = "deactivate")]
    private int[]? Selected { get; set; }

    protected override async Task OnInitializedAsync()
    {
        suppliers = await directory.GetAllAsync();
        activeCount = suppliers.Count(s => s.IsActive);
        notice = SupplierNotices.Find(Done);

        if (DeleteId is int id)
        {
            toDelete = await directory.FindByIdAsync(id);

            if (toDelete is null)
            {
                navigation.NavigateTo("suppliers?done=missing");
                return;
            }
        }
    }

    private async Task DeleteSupplier(bool confirmed)
    {
        // Blazor calls this only while the page renders the form,
        // and the page renders it only when toDelete is set.
        var id = toDelete!.Id;

        // The browser insists on the checkbox. A hand-made POST does
        // not, so the server checks again.
        if (!confirmed)
        {
            navigation.NavigateTo($"suppliers?delete={id}");
            return;
        }

        var result = await directory.DeleteAsync(id);
        var done = result switch
        {
            SupplierChangeResult.Done => "deleted",
            _ => "missing",
        };
        navigation.NavigateTo($"suppliers?done={done}");
    }

    private async Task DeactivateSelected()
    {
        // No ticked box means no value in the POST, and the binder
        // sets the property to null.
        var ids = Selected ?? [];

        if (ids.Length == 0)
        {
            notice = new(NoticeKind.Error,
                "Select at least one supplier.");
            return;
        }

        await directory.DeactivateAsync(ids);
        navigation.NavigateTo("suppliers?done=deactivated");
    }
}
