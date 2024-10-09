using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Output;
using NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class TestBed
{
    public IServiceProvider GetServiceProvider(string configurationFile)
    {
        var configuration = configurationFile.EndsWith(".json")
            ? new ConfigurationBuilder().AddJsonFile(configurationFile).Build()
            : new ConfigurationBuilder().AddYamlFile(configurationFile).Build();

        var collection = new ServiceCollection().AddConsoleIO().UseConsoleIO(configuration);
        if (AccountsRepository is not null)
        {
            collection.AddSingleton(AccountsRepository);
        }

        if (StreamProvider is not null)
        {
            collection.AddSingleton(StreamProvider);
        }

        return collection.BuildServiceProvider();
    }

    public IBudgetsRepository? AccountsRepository { get; set; }

    public IOutputStreamProvider? StreamProvider { get; set; } = new FakeStreamsProvider();

    internal IOperationsReader GetCsvParser(string configurationFile) => GetServiceProvider(configurationFile).GetRequiredService<IOperationsReader>();
}