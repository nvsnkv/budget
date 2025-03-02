using NVs.Budget.Application;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.UseCases;
using NVs.Budget.Controllers.Web;
using NVs.Budget.Infrastructure.ExchangeRates.CBRF;
using NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;
using NVs.Budget.Infrastructure.Persistence.EF;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Utilities.Expressions;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(b => b.AddSerilog(dispose: true));
var identityConnectionString = builder.Configuration.GetConnectionString("IdentityContext") ?? throw new InvalidOperationException("No connection string found for BudgetContext!");
var contentConnectionString = builder.Configuration.GetConnectionString("BudgetContext") ?? throw new InvalidOperationException("No connection string found for BudgetContext!");

var yandexAuthConfig = builder.Configuration.GetSection("Auth:Yandex").Get<YandexAuthConfig>() ?? throw new InvalidOperationException("No Auth config found for Yandex provider!");
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
        if (builder.Environment.IsDevelopment())
        {
            opts.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        }
    })
    .AddWebControllers();


var app = builder.Build();
app.UseYandexAuth("/");
app.UseCors();
app.MapGet("/", () => "OK");
app.MapGet("/admin/patch-db", async (IEnumerable<IDbMigrator> migrators, CancellationToken ct) =>
{
    foreach (var migrator in migrators)
    {
        await migrator.MigrateAsync(ct);
    }
});

app.MapControllers();

app.Run();
