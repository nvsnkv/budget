using System.Text;
using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output.Budgets;
using NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class CsvReaderShould
{
    private readonly Fixture _fixture = new();
    private readonly FakeStreamsProvider _streams = new();
    private readonly YamlBasedCsvReadingOptionsReader _reader;
    private readonly YamlBasedCsvReadingOptionsWriter _writer;

    public CsvReaderShould()
    {
        var opts = new Mock<IOptionsSnapshot<OutputOptions>>();
        opts.Setup(o => o.Value).Returns(new OutputOptions());

        _reader = new YamlBasedCsvReadingOptionsReader();
        _writer = new YamlBasedCsvReadingOptionsWriter(_streams, opts.Object);
    }

    [Fact]
    public async Task BeAbleToReadFromWriter()
    {
        var expected = _fixture.Create<CsvReadingOptions>();

        await _writer.Write(expected, CancellationToken.None);
        var outputBytes = _streams.GetOutputBytes();
        var text = Encoding.Default.GetString(outputBytes);
        _streams.ResetInput(outputBytes);

        var input = await _streams.GetInput();
        var result = await _reader.ReadFrom(input.Value, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Should().BeEquivalentTo(expected);

    }
}
