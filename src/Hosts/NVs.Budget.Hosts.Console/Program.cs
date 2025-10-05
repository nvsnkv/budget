// See https://aka.ms/new-console-template for more information

using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVs.Budget.Application;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.UseCases;
using NVs.Budget.Controllers.Console.Handlers;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Hosts.Console;
using NVs.Budget.Hosts.Console.Commands;
using NVs.Budget.Infrastructure.ExchangeRates.CBRF;
using NVs.Budget.Infrastructure.Files.CSV;
using NVs.Budget.Infrastructure.Identity.Console;
using NVs.Budget.Infrastructure.IO.Console;
using NVs.Budget.Infrastructure.Persistence.EF;
using NVs.Budget.Utilities.Expressions;
using Serilog;

Console.OutputEncoding = Encoding.UTF8;

var cancellationHandler = new ConsoleCancellationHandler();

var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");

var configurationDirectoryPath = Environment.GetEnvironmentVariable("BUDGET_CONFIGURATION_PATH") ?? "conf.d";

if (Directory.Exists(configurationDirectoryPath))
{
    var configs = ((string[]) ["*.json", "*.yml", "*.yaml"])
        .SelectMany(p => Directory.EnumerateFiles(configurationDirectoryPath, p))
        .Order();

    foreach (var file in configs)
    {
        if (file.EndsWith(".json"))
        {
            configurationBuilder.AddJsonFile(file);
        }
        else if (file.EndsWith(".yml") || file.EndsWith(".yaml"))
        {
            configurationBuilder.AddYamlFile(file);
        }
    }
}

var configuration = configurationBuilder
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? string.Empty}.json", true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

ReadableExpressionsParser.Default.RegisterAdditionalTypes(typeof(Tag));
var connectionString = configuration.GetConnectionString("BudgetContext") ?? throw new InvalidOperationException("No connection string found for BudgetContext!");

var collection = new ServiceCollection().AddConsoleIdentity()
    .AddLogging(builder => builder.AddSerilog(dispose: true))
    .AddSingleton(configuration)
    .AddMediatR(c => c.RegisterServicesFromAssembly(typeof(AdminVerb).Assembly))
    .AddEfCorePersistence(
        connectionString,
        ReadableExpressionsParser.Default
    )
    .AddCsvFiles(connectionString)
    .AddScoped<UserCache>()
    .AddScoped(p => p.GetRequiredService<UserCache>().CachedUser)
    .AddScoped<UserCacheInitializer>()
    .AddTransient<AppServicesFactory>()
    .AddTransient(p => p.GetRequiredService<AppServicesFactory>().CreateAccountant())
    .AddTransient(p => p.GetRequiredService<AppServicesFactory>().CreateAccountManager())
    .AddTransient(p => p.GetRequiredService<AppServicesFactory>().CreateReckoner())
    .AddApplicationUseCases()
    .AddSingleton(new Factory().CreateProvider())
    .AddConsole()
    .AddConsoleIO()
    .UseConsole(configuration)
    .UseConsoleIO(configuration);

var services = collection.BuildServiceProvider();

var factory = services.GetRequiredService<IServiceScopeFactory>();
using var scope = factory.CreateScope();

var initializer = scope.ServiceProvider.GetRequiredService<UserCacheInitializer>();
await initializer.TryInitializeCache(cancellationHandler.Token);

var entryPoint = scope.ServiceProvider.GetRequiredService<IEntryPoint>();
return await entryPoint.Process(args, cancellationHandler.Token);
