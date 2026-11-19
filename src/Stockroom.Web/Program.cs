using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PdfSharp.Fonts;
using Serilog;
using Stockroom.Core;
using Stockroom.Core.Data;
using Stockroom.Core.Lifetimes;
using Stockroom.Core.Services;
using Stockroom.Web.Components;
using Stockroom.Web.Endpoints;
using Stockroom.Web.Hosting;
using Stockroom.Web.Identity;
using Stockroom.Web.Maintenance;
using Stockroom.Web.Questions;

// The real logger needs configuration. Until it exists, this one
// catches every start failure.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(LoggingSetup.GetStartupLogPath(),
        rollingInterval: RollingInterval.Month)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    var folders = builder.AddDataFolders();
    builder.AddStockroomDefaults(folders);
    builder.AddStockroomLogging();
    builder.Services.AddStockroomOptions(builder.Configuration);
    builder.Services.AddHealthChecks();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Lets EditForm validate the rows of a list, not only the top
    // level.
    builder.Services.AddValidation();

    // Every "today" in Stockroom is a day in this time zone. The id
    // comes from the options, so ValidateOnStart reports a wrong
    // one together with every other wrong value.
    builder.Services.AddBusinessClock(services => services
        .GetRequiredService<IOptions<StockroomOptions>>()
        .Value.BusinessTimeZone);

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
    builder.Services.AddScoped<SubscriptionService>();
    builder.Services.AddOrderQuestions(builder.Configuration);

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
                services
                    .GetRequiredService<IHostApplicationLifetime>()
                    .ApplicationStopping,
                services.GetRequiredService<
                    ILogger<StockCheckCoordinator>>()));
    }

    var app = builder.Build();

    // Before the first step that can fail, so the log of a failed
    // start still says where each value came from.
    app.LogConfigurationSources();

    await app.MigrateDatabaseAsync();
    await app.StartReadOnlyRuleAsync();
    await app.EnsureAccountsAsync();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStatusCodePagesWithReExecute(
        "/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();

    // Before authorization, which would otherwise send a protected
    // page to the sign-in page first. So the two calls that
    // WebApplication adds by itself are written out here.
    app.UseMiddleware<MaintenanceMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UsePasskeyGate();
    app.UseAntiforgery();
    app.UseMiddleware<PasswordChangeMiddleware>();

    app.MapHealthChecks("/health");
    app.MapStaticAssets();
    app.MapOrderExport();
    app.MapFileEndpoints();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();
    app.MapAdditionalIdentityEndpoints();
    app.MapStockroomApi();

    await app.RunAsync();
    return 0;
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Stockroom could not start");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
