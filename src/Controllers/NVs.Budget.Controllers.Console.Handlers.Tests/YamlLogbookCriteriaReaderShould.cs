using System.Text;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Controllers.Console.Handlers.Criteria;
using NVs.Budget.Controllers.Console.Handlers.Criteria.Logbook;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Controllers.Console.Handlers.Tests;

public class YamlLogbookRulesetReaderShould
{
    private static readonly CriteriaParser Parser = new();
    private readonly YamlLogbookRulesetReader _reader = new(Parser, new(Parser));

    [Fact]
    public async Task ParseValidYamlConfig()
    {
        var yaml = @"
odds:
  tags: [ odd ]
  subcriteria:
    incomes: 
      predicate: o.Amount.Amount > 0
    else:
evens:
  tags:
    - odd
    - excluded
  mode: excluding
  subcriteria:
    subst:
      substitution: Year ${o.Timestamp.Year.ToString()}   
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
        result.Value.Should().BeEquivalentTo(expected);
    }
}
