using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;
using NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class TestBed
{
    public IServiceProvider GetServiceProvider()
    {
        var configuration = new ConfigurationBuilder().Build();

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

    internal IBudgetsRepository? AccountsRepository { get; set; }

    internal IOutputStreamProvider? StreamProvider { get; set; } = new FakeStreamsProvider();

    internal IOperationsReader GetCsvParser() => GetServiceProvider().GetRequiredService<IOperationsReader>();

    internal async Task<CsvReadingOptions> GetOptionsFrom(string file)
    {
        using var stream = new StreamReader(File.OpenRead(file));
        var reader = GetServiceProvider().GetRequiredService<ICsvReadingOptionsReader>();

        var result = await reader.ReadFrom(stream, CancellationToken.None);
        return result.Value;
    }


}
