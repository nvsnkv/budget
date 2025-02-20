using NVs.Budget.Application;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.UseCases;
using NVs.Budget.Controllers.Web.AspNetCore.Identity;
using NVs.Budget.Infrastructure.ExchangeRates.CBRF;
using NVs.Budget.Infrastructure.Persistence.EF.AspNetCore.Identity;
using NVs.Budget.Infrastructure.Persistence.EF;
using NVs.Budget.Infrastructure.Persistence.EF.Common;
using NVs.Budget.Utilities.Expressions;
using Serilog;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging(b => b.AddSerilog(dispose: true));
builder.Services.AddEfCorePersistence(
        builder.Configuration.GetConnectionString("BudgetContext") ?? throw new InvalidOperationException("No connection string found for BudgetContext!"),
        ReadableExpressionsParser.Default
    )
    .AddAspNetCoreIdentityPersistence(builder.Configuration.GetConnectionString("IdentityContext") ?? throw new InvalidOperationException("No connection string found for BudgetContext!"))
    .AddAspNetCoreIdentityUI()
    .AddScoped<UserCache>()
    .AddScoped<IUser>(p => p.GetRequiredService<UserCache>().CachedUser)
    .AddTransient<AppServicesFactory>()
    .AddTransient<IAccountant>(p => p.GetRequiredService<AppServicesFactory>().CreateAccountant())
    .AddTransient<IBudgetManager>(p => p.GetRequiredService<AppServicesFactory>().CreateAccountManager())
    .AddTransient<IReckoner>(p => p.GetRequiredService<AppServicesFactory>().CreateReckoner())
    .AddApplicationUseCases()
    .AddSingleton(new Factory().CreateProvider());

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "OK");

app.Run();
