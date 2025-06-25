using System.Text;
using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;
using NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class YamlLogbookRulesetReaderShould
{
    private readonly Fixture _fixture = new() { Customizations = { new ReadableExpressionsBuilder() } };
    private readonly FakeStreamsProvider _streams = new();
    private readonly YamlBasedLogbookCriteriaReader _reader = new(ReadableExpressionsParser.Default);
    private readonly YamlBasedLogbookCriteriaWriter _writer;

    public YamlLogbookRulesetReaderShould()
    {
        var opts = new Mock<IOptionsSnapshot<OutputOptions>>();
        opts.Setup(o => o.Value).Returns(new OutputOptions());
        _writer = new YamlBasedLogbookCriteriaWriter(_streams, opts.Object);
    }

    [Fact]
    public async Task ParseValidYamlConfig()
    {
        var yaml = @"
odds:
  tags: [ odd ]
  subcriteria:
    incomes: 
      predicate: o=> o.Amount.Amount > 0
    else:
evens:
  tags:
    - odd
    - excluded
  type: excluding
  subcriteria:
    subst:
      substitution: o => ""Year"" + o.Timestamp.Year.ToString()   
";
        var bytes = Encoding.UTF8.GetBytes(yaml);

        var expected = new UniversalCriterion(string.Empty, [
            new TagBasedCriterion("odds", [new("odd")], TagBasedCriterionType.Including, [
                new PredicateBasedCriterion("incomes", o => o.Amount.Amount > 0, []),
                new UniversalCriterion("else")
            ]),
            new TagBasedCriterion("evens", [new("odd"), new("excluded")], TagBasedCriterionType.Excluding,[
                new SubstitutionBasedCriterion("subst", o => $"Year {o.Timestamp.Year}")
            ])
        ]);

        using var streamReader = new StreamReader(new MemoryStream(bytes));
        var result = await _reader.ReadFrom(streamReader, CancellationToken.None);
        result.Should().BeSuccess();
        result.Value.GetCriterion().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ReadCriteriaWrittenByWriter()
    {
        var logbook = new LogbookCriteria(string.Empty,
            [
                new("odds", [
                    new LogbookCriteria("incomes", null, null, null,  null,
                        ReadableExpressionsParser.Default.ParseUnaryPredicate<Operation>("o=> o.Amount.Amount > 0").Value, null),
                    new LogbookCriteria("else", null, null, null, null, null, true)
                ],
                TagBasedCriterionType.Including, [new("odd")],
                null, null, null),
                new LogbookCriteria("evens",
                    [
                        new LogbookCriteria("subst", null, null, null,
                            ReadableExpressionsParser.Default.ParseUnaryConversion<Operation>("o => \"Year\" + o.Timestamp.Year.ToString()").Value,
                            null, null)
                    ],
                    TagBasedCriterionType.Excluding, [new("odd"), new("excluded")], null, null, null)
            ],
            null, null, null, null, true);

        await _writer.Write(logbook, CancellationToken.None);

        var data = _streams.GetOutputBytes();
        var text = Encoding.UTF8.GetString(data);

        using var streamReader = new StreamReader(new MemoryStream(data));

        var result = await _reader.ReadFrom(streamReader, CancellationToken.None);
        result.Should().BeSuccess();
        result.Value.Should().BeEquivalentTo(logbook);
    }

}
