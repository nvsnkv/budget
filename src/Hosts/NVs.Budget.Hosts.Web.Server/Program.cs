using NVs.Budget.Application;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.UseCases;
using NVs.Budget.Controllers.Web;
using NVs.Budget.Infrastructure.ExchangeRates.CBRF;
using NVs.Budget.Infrastructure.Files.CSV;
using NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;
using NVs.Budget.Infrastructure.Persistence.EF;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Utilities.Expressions;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(Log.Logger);

builder.Services.AddLogging(b => b.AddSerilog(dispose: true));
var identityConnectionString = builder.Configuration.GetConnectionString("IdentityContext") ?? throw new InvalidOperationException("No connection string found for IdentityContext!");
var contentConnectionString = builder.Configuration.GetConnectionString("BudgetContext") ?? throw new InvalidOperationException("No connection string found for BudgetContext!");
var yandexAuthConfig = builder.Configuration.GetSection("Auth:Yandex").Get<YandexAuthConfig>() ?? throw new InvalidOperationException("No Auth config found for Yandex provider!");
var frontendUrl = builder.Configuration.GetSection("FrontendUrl").Get<string>() ?? throw new InvalidOperationException("No FrontendUrl config found!");

builder.Services
    .AddEfCorePersistence(
        contentConnectionString,
        ReadableExpressionsParser.Default
    )
    .AddYandexAuth(yandexAuthConfig, identityConnectionString)
    .AddScoped<UserCache>()
    .AddScoped<IUser>(p => p.GetRequiredService<UserCache>().CachedUser)
    .AddTransient<AppServicesFactory>()
    .AddTransient<IAccountant>(p => p.GetRequiredService<AppServicesFactory>().CreateAccountant())
    .AddTransient<IBudgetManager>(p => p.GetRequiredService<AppServicesFactory>().CreateAccountManager())
    .AddTransient<IReckoner>(p => p.GetRequiredService<AppServicesFactory>().CreateReckoner())
    .AddApplicationUseCases()
    .AddSingleton(new Factory().CreateProvider())
    .AddCors(opts =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string>() ?? string.Empty;
        opts.AddDefaultPolicy(b => b.WithOrigins(allowedOrigins.Split(';')).AllowCredentials().AllowAnyHeader().AllowAnyMethod());
    })
    .AddCsvFiles(contentConnectionString)
    .AddSingleton(ReadableExpressionsParser.Default)
    .AddWebControllers();

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseYandexAuth(frontendUrl)
    .UseWebControllers(app.Environment.IsDevelopment());

app.UseCors();
app.MapGet("/", () => Results.Redirect(frontendUrl));
app.MapGet("/admin/patch-db", async (IEnumerable<IDbMigrator> migrators, CancellationToken ct) =>
{
    foreach (var migrator in migrators)
    {
        await migrator.MigrateAsync(ct);
    }
});

app.MapControllers();

app.Run();
