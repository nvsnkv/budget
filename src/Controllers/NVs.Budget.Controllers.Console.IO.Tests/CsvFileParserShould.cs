using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Controllers.Console.IO.Tests.TestData;

namespace NVs.Budget.Controllers.Console.IO.Tests;

public class CsvFileParserShould(TestBed testBed) : IClassFixture<TestBed>
{
    [Fact]
    public async Task ParseValidFile()
    {
        var parser = testBed.GetCsvParser("TestData\\ValidFile\\validFileConfig.json");
        var stream = File.OpenRead("TestData\\ValidFile\\validFile.csv");

        var operations = await parser.ReadUnregisteredOperations(new StreamReader(stream), "validFile.csv", CancellationToken.None).ToListAsync();
        operations.Should().AllSatisfy(r => r.Should().BeSuccess());
        operations.Select(o => o.Value).Should().BeEquivalentTo(ValidFile.Operations);
    }
}
