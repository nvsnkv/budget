using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;

namespace NVs.Budget.Controllers.Console.Handlers.Tests;

public class CriteriaParserShould
{
    private readonly CriteriaParser _parser = new();
    private readonly Fixture _fixture = new();

    [Fact]
    public void ParseExpressionsThatUsesMoney()
    {
        var action = () =>
        {
            var expression = _parser.ParseTransferCriteria(
                "l.Amount.IsNegative() && r.Amount.IsPositive() && l.Amount.Abs() == r.Amount.Abs() && l.Description == r.Description && l.Budget != r.Budget && (l.Timestamp - r.Timestamp).Duration() < TimeSpan.FromMinutes(2)");
            var func = expression.Compile();

            func(_fixture.Create<TrackedOperation>(), _fixture.Create<TrackedOperation>());
        };

        action.Should().NotThrow();
    }
}
