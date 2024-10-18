using System.Text;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class YamlLogbookRulesetReaderShould
{
    private readonly YamlLogbookRulesetReader _reader = new(ReadableExpressionsParser.Default);

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
  mode: excluding
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
}
