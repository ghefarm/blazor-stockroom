using Microsoft.EntityFrameworkCore;
using PdfSharp.Fonts;
using Stockroom.Core;
using Stockroom.Core.Data;
using Stockroom.Core.Lifetimes;
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
builder.Services.AddHybridCache();
builder.Services.AddScoped<ProductSummaryService>();

var archiveFolder = Path.GetFullPath(
    builder.Configuration["Imports:ArchiveFolder"]
        ?? "App_Data/price-lists",
    builder.Environment.ContentRootPath);
builder.Services.AddSingleton(new ImportArchive(archiveFolder));
builder.Services.AddScoped<PriceListImporter>();

// PDFsharp looks for fonts only where it is told to.
GlobalFontSettings.UseWindowsFontsUnderWindows = true;

// Three markers that only show their lifetime on a page.
builder.Services.AddSingleton<SingletonMarker>();
builder.Services.AddScoped<ScopedMarker>();
builder.Services.AddTransient<TransientMarker>();

// The slow work itself. A singleton, because the coordinator
// uses it after the circuit that started a run is gone.
builder.Services.AddSingleton<IStockCheckWork>(
    services => new StockCheckWork(
        services.GetRequiredService<
            IDbContextFactory<StockroomDbContext>>(),
        TimeSpan.FromSeconds(1.5)));

// Two designs behind the same page. The setting chooses one.
var useQueue = builder.Configuration
    .GetValue<bool>("StockCheck:UseQueue");
if (useQueue)
{
    builder.Services.AddStockCheckWorker();
}
else
{
    builder.Services.AddSingleton<StockCheckStatus>();
    builder.Services.AddSingleton<IStockCheckRunner>(
        services => new StockCheckCoordinator(
            services.GetRequiredService<StockCheckStatus>(),
            services.GetRequiredService<IStockCheckWork>(),
            services.GetRequiredService<IHostApplicationLifetime>()
                .ApplicationStopping,
            services.GetRequiredService<
                ILogger<StockCheckCoordinator>>()));
}

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
