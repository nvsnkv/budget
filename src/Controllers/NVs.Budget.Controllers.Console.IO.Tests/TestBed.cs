using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.IO.Input.CsvOperationsReader;

namespace NVs.Budget.Controllers.Console.IO.Tests;

public class TestBed
{
    public IServiceProvider GetServiceProvider(string configurationFile)
    {
        var configuration = new ConfigurationBuilder().AddJsonFile(configurationFile).Build();

        return new ServiceCollection().AddConsoleIO().UseConsoleIO(configuration).BuildServiceProvider();
    }

    internal IOperationsReader GetCsvParser(string configurationFile) => GetServiceProvider(configurationFile).GetRequiredService<IOperationsReader>();
}
