// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NVs.Budget.Application;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.UseCases;
using NVs.Budget.Controllers.Console;
using NVs.Budget.Hosts.Console;
using NVs.Budget.Infrastructure.ExchangeRates.CBRF;
using NVs.Budget.Infrastructure.Identity.Console;
using NVs.Budget.Infrastructure.Persistence.EF;
using NVs.Budget.Infrastructure.Persistence.EF.Context;

var cancellationHandler = new ConsoleCancellationHandler();

var builder = Host.CreateApplicationBuilder(args);
builder.Services
    .AddConsoleIdentity()
    .AddEfCorePersistence(builder.Configuration.GetConnectionString("BudgetContext") ?? throw new InvalidOperationException("No connection string found for BudgetContext!"));

var provider = builder.Services.BuildServiceProvider();
await using (var migrator = provider.GetRequiredService<IDbMigrator>())
{
    await migrator.MigrateAsync(cancellationHandler.Token);
}

var userCache = provider.GetRequiredService<UserCache>();

await userCache.EnsureInitialized(cancellationHandler.Token);
builder.Services.AddSingleton(userCache)
    .AddTransient<AppServicesFactory>()
    .AddTransient<IAccountant>(p => p.GetRequiredService<AppServicesFactory>().CreateAccountant())
    .AddTransient<IAccountManager>(p => p.GetRequiredService<AppServicesFactory>().CreateAccountManager())
    .AddTransient<IReckoner>(p => p.GetRequiredService<AppServicesFactory>().CreateReckoner())
    .AddApplicationUseCases();

var cbrfFactory = new Factory();
builder.Services.AddSingleton(cbrfFactory.CreateProvider());

builder.Services.AddConsole();
builder.Services.UseConsole(builder.Configuration);

var host = builder.Build();

var entryPoint = host.Services.GetRequiredService<IEntryPoint>();
return await entryPoint.Process(args, cancellationHandler.Token);
