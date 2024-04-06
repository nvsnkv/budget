// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Application;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.UseCases;
using NVs.Budget.Controllers.Console;
using NVs.Budget.Hosts.Console;
using NVs.Budget.Hosts.Console.Commands;
using NVs.Budget.Infrastructure.ExchangeRates.CBRF;
using NVs.Budget.Infrastructure.Identity.Console;
using NVs.Budget.Infrastructure.Persistence.EF;

var cancellationHandler = new ConsoleCancellationHandler();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("tagging_rules.json", true)
    .AddJsonFile("transfer_rules.json", true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty}.json", true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var collection = new ServiceCollection().AddConsoleIdentity()
    .AddMediatR(c => c.RegisterServicesFromAssembly(typeof(AdminVerb).Assembly))
    .AddEfCorePersistence(configuration.GetConnectionString("BudgetContext") ?? throw new InvalidOperationException("No connection string found for BudgetContext!"))
    .AddScoped<UserCache>()
    .AddTransient<AppServicesFactory>()
    .AddTransient<IAccountant>(p => p.GetRequiredService<AppServicesFactory>().CreateAccountant())
    .AddTransient<IAccountManager>(p => p.GetRequiredService<AppServicesFactory>().CreateAccountManager())
    .AddTransient<IReckoner>(p => p.GetRequiredService<AppServicesFactory>().CreateReckoner())
    .AddApplicationUseCases()
    .AddSingleton(new Factory().CreateProvider())
    .AddConsole()
    .UseConsole(configuration);

var services = collection
    .BuildServiceProvider();

var factory = services.GetRequiredService<IServiceScopeFactory>();
using var scope = factory.CreateScope();

var userCache = scope.ServiceProvider.GetRequiredService<UserCache>();
await userCache.EnsureInitialized(cancellationHandler.Token);

var entryPoint = scope.ServiceProvider.GetRequiredService<IEntryPoint>();
return await entryPoint.Process(args, cancellationHandler.Token);
