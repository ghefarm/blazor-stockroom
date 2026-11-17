using PdfSharp.Fonts;
using Stockroom.Core;
using Stockroom.Core.Services;
using Stockroom.Web.Components;
using Stockroom.Web.Endpoints;
using Stockroom.Web.Hosting;
using Stockroom.Web.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Lets EditForm validate the rows of a list, not only the top level.
builder.Services.AddValidation();

// Every "today" in Stockroom is a day in this time zone.
builder.Services.AddBusinessClock("America/Chicago");

builder.AddStockroomAuditing();

builder.AddStockroomDatabase();
builder.AddStockroomIdentity();
builder.Services.AddScoped<ProductCatalog>();
builder.Services.AddScoped<SupplierDirectory>();
builder.Services.AddScoped<PurchaseOrderService>();
builder.Services.AddScoped<OrderListService>();
builder.Services.AddScoped<OrderActivity>();

var archiveFolder = Path.GetFullPath(
    builder.Configuration["Imports:ArchiveFolder"]
        ?? "App_Data/price-lists",
    builder.Environment.ContentRootPath);
builder.Services.AddSingleton(new ImportArchive(archiveFolder));
builder.Services.AddScoped<PriceListImporter>();

// PDFsharp looks for fonts only where it is told to.
GlobalFontSettings.UseWindowsFontsUnderWindows = true;

var app = builder.Build();

await app.MigrateDatabaseAsync();
await app.EnsureAccountsAsync();

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

app.UsePasskeyGate();
app.UseAntiforgery();
app.UseMiddleware<PasswordChangeMiddleware>();

app.MapStaticAssets();
app.MapOrderExport();
app.MapFileEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();
app.MapStockroomApi();

app.Run();
