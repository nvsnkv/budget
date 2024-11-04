using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Output;
using NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;
using NVs.Budget.Infrastructure.IO.Console.Tests.TestData.FileWithDotsInNumbersAndCyrillicAttributes;
using NVs.Budget.Infrastructure.IO.Console.Tests.TestData.ValidFile;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class CsvFileParserShould(TestBed testBed) : IClassFixture<TestBed>
{
    private readonly Fixture _fixture = new() { Customizations = { new ReadableExpressionsBuilder() }};

    [Fact]
    public async Task ParseValidFile()
    {
        testBed.AccountsRepository = new FakeReadOnlyBudgetsRepository([]);
        var parser = testBed.GetCsvParser();
        var options = await testBed.GetOptionsFrom("TestData/ValidFile/validFileConfig.yml");
        var stream = File.OpenRead("TestData/ValidFile/validFile.csv");

        var name = "validFile.csv";
        var operations = await parser.ReadUnregisteredOperations(new StreamReader(stream), options.GetFileOptionsFor(name).Value, CancellationToken.None).ToListAsync();
        operations.Should().AllSatisfy(r => r.Should().BeSuccess($"{r.PrintoutReasons()}"));
        operations.Select(o => o.Value).Should().BeEquivalentTo(ValidFile.Operations);
    }

    [Fact]
    public async Task ParseFileWithDotsInNumbersAndCyrillicComments()
    {
        testBed.AccountsRepository = new FakeReadOnlyBudgetsRepository([]);
        var parser = testBed.GetCsvParser();
        var options = await testBed.GetOptionsFrom("TestData/FileWithDotsInNumbersAndCyrillicAttributes/file.yml");
        var stream = File.OpenRead("TestData/FileWithDotsInNumbersAndCyrillicAttributes/file.csv");

        var name = "file.csv";
        var operations = await parser.ReadUnregisteredOperations(new StreamReader(stream), options.GetFileOptionsFor(name).Value, CancellationToken.None).ToListAsync();
        operations.Should().AllSatisfy(r => r.Should().BeSuccess($"{r.PrintoutReasons()}"));
        operations.Select(o => o.Value).Should().BeEquivalentTo(FileWithDotsInNumbersAndCyrillicAttributes.Operations);
    }

    [Fact]
    public async Task ParseTrackedTransactionsFileSuccessfully()
    {
        _fixture.Inject(LogbookCriteria.Universal);
        var budgets = _fixture.Create<Generator<TrackedBudget>>().Take(2).ToArray();
        var operations = new List<TrackedOperation>();

        foreach (var account in budgets)
        {
            using (_fixture.SetAccount(account))
            {
                operations.AddRange(_fixture.Create<Generator<TrackedOperation>>().Take(10));
            }
        }

        testBed.AccountsRepository = new FakeReadOnlyBudgetsRepository(budgets);

        await using var streams = new FakeStreamsProvider();
        testBed.StreamProvider = streams;

        var writer = testBed.GetServiceProvider().GetRequiredService<IObjectWriter<TrackedOperation>>();

        await writer.Write(operations, CancellationToken.None);
        var data = streams.GetOutputBytes();

        await using var stream = new MemoryStream(data);
        using var reader = new StreamReader(stream);
        var parser = testBed.GetCsvParser();

        var actual = await parser.ReadTrackedOperation(reader, CancellationToken.None).ToListAsync();
        actual.Should().AllSatisfy(r => r.Should().BeSuccess());
        actual.Count.Should().Be(operations.Count);
        actual.Select(r => r.Value).Should().BeEquivalentTo(operations);
    }
}
