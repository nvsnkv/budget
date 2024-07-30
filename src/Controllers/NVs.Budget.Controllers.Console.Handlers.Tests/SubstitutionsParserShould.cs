using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Handlers.Criteria;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Controllers.Console.Handlers.Tests;

public class SubstitutionsParserShould
{
    private readonly TrackedOperation _operation = new Fixture().Create<TrackedOperation>();
    private readonly SubstitutionsParser _parser = new(new CriteriaParser());

    [Fact]
    public void ReturnIncomingStringIfNoSubstitutionNeeded()
    {
        var value = new Fixture().Create<string>();
        var result = _parser.GetSubstitutions<TrackedOperation>(value, "arg");

        result(_operation).Should().Be(value);
    }

    [Fact]
    public void SubstituteValuesInTheMiddleOfTheString()
    {
        var o = _operation;
        var expected = $"description: {o.Description} ({o.Amount.CurrencyCode.ToString()})";
        var pattern = "description: ${o.Description} (${o.Amount.CurrencyCode.ToString()})";

        var result = _parser.GetSubstitutions<TrackedOperation>(pattern, "o");
        result(o).Should().Be(expected);
    }

}
