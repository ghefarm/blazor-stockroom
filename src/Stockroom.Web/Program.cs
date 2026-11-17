using Stockroom.Core.Services;
using Stockroom.Web.Components;
using Stockroom.Web.Endpoints;
using Stockroom.Web.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Lets EditForm validate the rows of a list, not only the top level.
builder.Services.AddValidation();

builder.AddStockroomDatabase();
builder.Services.AddScoped<ProductCatalog>();
builder.Services.AddScoped<SupplierDirectory>();
builder.Services.AddScoped<PurchaseOrderService>();
builder.Services.AddScoped<OrderListService>();

var app = builder.Build();

await app.MigrateDatabaseAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change
    // this for production scenarios, see
    // https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute(
    "/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapOrderExport();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
