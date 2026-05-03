using FluentAssertions;
using NMoneys;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Aggregates.Plans;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Tests;

public class BudgetPlanShould
{
    private static readonly Currency Currency = Currency.Get(CurrencyIsoCode.RUB);

    [Fact]
    public void RejectExpectationsOutsidePlanRange()
    {
        var criterion = new UniversalCriterion("All");
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        var expectation = new PlanExpectation(Guid.NewGuid(), new Money(100, Currency), from.AddDays(-1), till);

        var act = () => new BudgetPlan(Guid.NewGuid(), "Monthly", from, till, null, criterion, Currency, [expectation]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*inside the plan range*");
    }

    [Fact]
    public void RejectExpectationsWithDifferentCurrency()
    {
        var criterion = new UniversalCriterion("All");
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        var expectation = new PlanExpectation(Guid.NewGuid(), new Money(100, Currency.Get(CurrencyIsoCode.USD)), from, till);

        var act = () => new BudgetPlan(Guid.NewGuid(), "Monthly", from, till, null, criterion, Currency, [expectation]);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*currency must match*");
    }

    [Fact]
    public void SumExpectationsForCriteriaTree()
    {
        var groceries = new TagBasedCriterion("Groceries", [new Tag("groceries")], TagBasedCriterionType.Including);
        var cafes = new TagBasedCriterion("Cafes", [new Tag("cafes")], TagBasedCriterionType.Including);
        var criterion = new UniversalCriterion("All", [groceries, cafes]);
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        var plan = new BudgetPlan(Guid.NewGuid(), "Monthly", from, till, null, criterion, Currency,
        [
            new PlanExpectation(Guid.NewGuid(), new Money(-100, Currency), from, till, "Groceries"),
            new PlanExpectation(Guid.NewGuid(), new Money(-50, Currency), from, till, "Cafes")
        ]);

        plan.GetExpectedAmount(criterion, from, till).Should().Be(new Money(-150, Currency));
        plan.GetExpectedAmount(groceries, from, till).Should().Be(new Money(-100, Currency));
        plan.GetExpectedAmount(cafes, from, till).Should().Be(new Money(-50, Currency));
    }

    [Fact]
    public void BuildVarianceReportWithActualAndPlannedOnlyCells()
    {
        var groceriesTag = new Tag("groceries");
        var cafesTag = new Tag("cafes");
        var groceries = new TagBasedCriterion("Groceries", [groceriesTag], TagBasedCriterionType.Including);
        var cafes = new TagBasedCriterion("Cafes", [cafesTag], TagBasedCriterionType.Including);
        var criterion = new UniversalCriterion("All", [groceries, cafes]);
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        var plan = new BudgetPlan(Guid.NewGuid(), "Monthly", from, till, null, criterion, Currency,
        [
            new PlanExpectation(Guid.NewGuid(), new Money(-100, Currency), from, till, "Groceries"),
            new PlanExpectation(Guid.NewGuid(), new Money(-50, Currency), from, till, "Cafes")
        ]);
        var logbook = new CriteriaBasedLogbook(criterion);

        logbook.Register(Operation(from.AddDays(1), new Money(-80, Currency), groceriesTag)).IsSuccess.Should().BeTrue();

        var report = new VarianceReport(plan, logbook, [new PlanRange("January", from, till)]);

        var root = report.Variances.Single();
        root.Expected.Should().Be(new Money(-150, Currency));
        root.Actual.Should().Be(new Money(-80, Currency));
        root.ActualComparison.Should().BeGreaterThan(0);

        var plannedOnly = root.Children.Single(v => v.Description == "Cafes");
        plannedOnly.Expected.Should().Be(new Money(-50, Currency));
        plannedOnly.Actual.Should().Be(new Money(0, Currency));
        plannedOnly.IsPlannedOnly.Should().BeTrue();
    }

    [Fact]
    public void NotDoubleCountActualOperationsOnAdjacentRangeBoundaries()
    {
        var criterion = new UniversalCriterion("All");
        var january = new DateTime(2026, 1, 1);
        var february = new DateTime(2026, 2, 1);
        var march = new DateTime(2026, 3, 1);
        var plan = new BudgetPlan(Guid.NewGuid(), "Monthly", january, march, "0 0 1 * *", criterion, Currency);
        var logbook = new CriteriaBasedLogbook(criterion);

        logbook.Register(Operation(february, new Money(-80, Currency))).IsSuccess.Should().BeTrue();

        var report = new VarianceReport(plan, logbook,
        [
            new PlanRange("January", january, february),
            new PlanRange("February", february, march)
        ]);

        report.Variances.Select(v => v.Actual).Should().Equal(
            new Money(0, Currency),
            new Money(-80, Currency));
    }

    private static Operation Operation(DateTime timestamp, Money amount, params Tag[] tags)
    {
        var owner = new Owner(Guid.NewGuid(), "Owner");
        var budget = new Domain.Entities.Budgets.Budget(Guid.NewGuid(), "Budget", [owner]);
        return new Operation(Guid.NewGuid(), timestamp, amount, "Operation", string.Empty, budget, tags, null);
    }
}
