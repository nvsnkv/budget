using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;
using NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class YamlBasedTransferCriteriaWriterShould
{
    private readonly Fixture _fixture = new() { Customizations = { new ReadableExpressionsBuilder() } };
    private readonly FakeStreamsProvider _streams = new();
    private readonly YamlBasedTransferCriteriaWriter _writer;
    private readonly YamlBasedTransferCriteriaReader _reader;

    public YamlBasedTransferCriteriaWriterShould()
    {
        var options = new Mock<IOptionsSnapshot<OutputOptions>>();
        options.SetupGet(x => x.Value).Returns(new OutputOptions());

        _writer = new YamlBasedTransferCriteriaWriter(_streams, options.Object);
        _reader = new YamlBasedTransferCriteriaReader(new());
    }

    [Fact]
    public async Task ReadRulesWrittenByWriter()
    {
        var criteria = _fixture.Create<Generator<TransferCriterion>>().Take(7).ToList();
        await _writer.Write(criteria, CancellationToken.None);

        var data = _streams.GetOutputBytes();

        using var streamReader = new StreamReader(new MemoryStream(data));

        var result = await _reader.ReadFrom(streamReader, CancellationToken.None).ToListAsync();
        result.Should().AllSatisfy(r => r.Should().BeSuccess());
        result.Select(r => r.Value).Should().BeEquivalentTo(criteria);
    }
}
