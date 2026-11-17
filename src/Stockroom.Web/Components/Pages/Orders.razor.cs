using Microsoft.AspNetCore.Components;
using Stockroom.Core.Entities;
using Stockroom.Core.Services;

namespace Stockroom.Web.Components.Pages;

public partial class Orders(
    OrderListService orderList,
    PurchaseOrderService orderService,
    NavigationManager navigation)
    : IDisposable
{
    private static readonly OrderStatus?[] tabs =
    [
        null,
        OrderStatus.Open,
        OrderStatus.PartlyDelivered,
        OrderStatus.Delivered,
        OrderStatus.Cancelled,
    ];

    private OrderListPage? list;
    private IReadOnlyList<OrderRow> rows = [];
    private string searchText = "";
    private CancellationTokenSource? pause;

    [SupplyParameterFromQuery(Name = OrderListQuery.SupplierName)]
    private string? Supplier { get; set; }

    [SupplyParameterFromQuery(Name = OrderListQuery.StatusName)]
    private string? Status { get; set; }

    [SupplyParameterFromQuery(Name = OrderListQuery.FromName)]
    private string? From { get; set; }

    [SupplyParameterFromQuery(Name = OrderListQuery.ToName)]
    private string? To { get; set; }

    [SupplyParameterFromQuery(Name = OrderListQuery.SearchName)]
    private string? Search { get; set; }

    [SupplyParameterFromQuery(Name = OrderListQuery.PageName)]
    private string? Page { get; set; }

    [SupplyParameterFromQuery(Name = OrderListQuery.SizeName)]
    private string? Size { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        OrderListQuery.TryRead(
            Supplier, Status, From, To, Search, out var requested);

        // Read as text: an int? property fails the whole page when
        // someone types page=abc into the address.
        var page = int.TryParse(Page, out var p) ? p : 1;
        var size = int.TryParse(Size, out var s)
            ? s
            : OrderListService.PageSizes[0];

        // Our own address change arrives here too. Load only when the
        // address asks for something the page does not show yet.
        if (list is not null && requested == list.Filter
            && page == list.Page && size == list.PageSize)
        {
            return;
        }

        await LoadAsync(requested, page, size);
        searchText = list!.Filter.Search ?? "";
    }

    private async Task ChangeAsync(
        OrderListFilter filter, int page, int size)
    {
        filter = filter.Normalize();
        if (filter == list!.Filter
            && page == list.Page && size == list.PageSize)
        {
            return;
        }

        // The list first, the address second. When the address comes
        // back to OnParametersSetAsync, it matches and loads nothing.
        await LoadAsync(filter, page, size);
        var values = OrderListQuery.Write(list.Filter);
        values[OrderListQuery.PageName] = list.Page;
        values[OrderListQuery.SizeName] = list.PageSize;
        navigation.NavigateTo(
            navigation.GetUriWithQueryParameters(values),
            replace: true);
    }

    private async Task LoadAsync(
        OrderListFilter filter, int page, int size)
    {
        var loaded = await orderList.LoadAsync(filter, page, size);

        // One more query for the product names, over the ids of the
        // rows on this page only.
        rows = await orderService.AddProductNamesAsync(loaded.Rows);
        list = loaded;
    }

    private Task ChangeFilterAsync(OrderListFilter filter) =>
        ChangeAsync(filter, 1, list!.PageSize);

    private Task SelectStatusAsync(OrderStatus? status) =>
        ChangeFilterAsync(list!.Filter with { Status = status });

    private Task SelectSupplierAsync(ChangeEventArgs e) =>
        ChangeFilterAsync(list!.Filter with
        {
            SupplierId = int.TryParse($"{e.Value}", out var id)
                ? id
                : null,
        });

    private Task SelectFromAsync(ChangeEventArgs e) =>
        ChangeFilterAsync(list!.Filter with { From = ReadDate(e) });

    private Task SelectToAsync(ChangeEventArgs e) =>
        ChangeFilterAsync(list!.Filter with { To = ReadDate(e) });

    private Task GoToPageAsync(int page) =>
        ChangeAsync(list!.Filter, page, list.PageSize);

    private Task SelectSizeAsync(ChangeEventArgs e)
    {
        var size = int.Parse($"{e.Value}");
        var page = list!.FindPageFor(size);
        return ChangeAsync(list.Filter, page, size);
    }

    private async Task SearchAfterPauseAsync()
    {
        // Every key cancels the wait of the key before it. Only a
        // pause of 300 ms reaches the database.
        pause?.Cancel();
        pause?.Dispose();
        pause = new CancellationTokenSource();
        try
        {
            await Task.Delay(300, pause.Token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await ChangeFilterAsync(
            list!.Filter with { Search = searchText });
    }

    private async Task ResetAsync()
    {
        searchText = "";
        await ChangeFilterAsync(OrderListFilter.Empty);
    }

    // A date input sends yyyy-MM-dd, or an empty text when cleared.
    private static DateOnly? ReadDate(ChangeEventArgs e) =>
        DateOnly.TryParseExact($"{e.Value}", "yyyy-MM-dd", out var d)
            ? d
            : null;

    private int CountFor(OrderStatus? status) => status is { } s
        ? list!.StatusCounts.GetValueOrDefault(s)
        : list!.StatusCounts.Values.Sum();

    private static string Describe(OrderStatus? status) =>
        status switch
        {
            null => "All",
            OrderStatus.PartlyDelivered => "Partly delivered",
            _ => status.Value.ToString(),
        };

    private string? FromText =>
        OrderListQuery.FormatDate(list!.Filter.From);

    private string? ToText =>
        OrderListQuery.FormatDate(list!.Filter.To);

    private string ExportLink =>
        navigation.GetUriWithQueryParameters(
            "orders/export", OrderListQuery.Write(list!.Filter));

    public void Dispose()
    {
        pause?.Cancel();
        pause?.Dispose();
    }
}
