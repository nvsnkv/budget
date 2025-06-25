using System.Text;
using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;
using NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class YamlBasedTaggingRuleReaderShould
{
    private readonly FakeStreamsProvider _streams = new();
    private readonly Fixture _fixture = new() { Customizations = { new ReadableExpressionsBuilder() }};
    private readonly YamlBasedTaggingCriteriaReader _reader = new(ReadableExpressionsParser.Default);
    private readonly YamlBasedTaggingCriteriaWriter _writer;

    public YamlBasedTaggingRuleReaderShould()
    {
        var options = new Mock<IOptionsSnapshot<OutputOptions>>();
        options.SetupGet(o => o.Value).Returns(new OutputOptions());

        _writer = new YamlBasedTaggingCriteriaWriter(_streams, options.Object);
    }


    [Fact]
    public async Task ReadValues()
    {
        var tags = new Dictionary<string, List<string>>()
        {
            { @"o => ""Expences""", ["o => o.Amount.IsNegative()", "o => o.Amount.Amount < 0"] },
            { @"o => ""Incomes""", ["o => o.Amount.IsPositive()", "o => o.Amount.Amount > 0"] },
            { "o => o.Timestamp.Month.ToString() + o.Timestamp.Year.ToString()", ["o => true"] }
        };

        var expected = tags.SelectMany(kv => kv.Value.Select(v => new TaggingCriterion(
            ReadableExpressionsParser.Default.ParseUnaryConversion<TrackedOperation>(kv.Key).Value,
            ReadableExpressionsParser.Default.ParseUnaryPredicate<TrackedOperation>(v).Value
        ))).ToList();
        var stream = GetStream(tags);

        var actual = await _reader.ReadFrom(new StreamReader(stream), CancellationToken.None).ToListAsync();

        actual.Should().AllSatisfy(r => r.Should().BeSuccess());
        actual.Select(r => r.Value).Should().BeEquivalentTo(expected);
    }

    private MemoryStream GetStream(Dictionary<string, List<string>> dict)
    {
        var builder = new StringBuilder();
        foreach (var (key, value) in dict)
        {
            builder.AppendLine($"{key}:");
            foreach (var val in value)
            {
                builder.AppendLine($"  - {val}");
            }
        }

        var text = builder.ToString();

        return new MemoryStream(Encoding.UTF8.GetBytes(text));
    }

    [Fact]
    public async Task ReadValuesWrittenByWriter()
    {
        var criteria = _fixture.Create<Generator<TaggingCriterion>>().Take(4).ToList();
        await _writer.Write(criteria, CancellationToken.None);

        var data = _streams.GetOutputBytes();

        using var streamReader = new StreamReader(new MemoryStream(data));

        var actual = await _reader.ReadFrom(streamReader, CancellationToken.None).ToListAsync();
        actual.Should().AllSatisfy(r => r.Should().BeSuccess());
        actual.Select(r => r.Value).Should().BeEquivalentTo(criteria);
    }
}
