using FluentAssertions;
using Moq;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Services.Accounting.Plans;
using NVs.Budget.Application.Tests.Fakes;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.Tests;

public class BudgetPlanManagerShould
{
    private static readonly Currency Currency = Currency.Get(CurrencyIsoCode.RUB);
    private readonly Owner _owner = new(Guid.NewGuid(), "Owner");
    private readonly FakeBudgetsRepository _budgets = new();
    private readonly FakeBudgetPlansRepository _plans = new();
    private readonly Mock<IReckoner> _reckoner = new();
    private readonly BudgetPlanManager _manager;

    public BudgetPlanManagerShould()
    {
        var user = new Mock<IUser>();
        user.Setup(u => u.AsOwner()).Returns(_owner);
        _manager = new BudgetPlanManager(_plans, _budgets, _reckoner.Object, user.Object);
    }

    [Fact]
    public async Task CreatePlansForOwnedBudget()
    {
        var budget = Budget();
        _budgets.Append([budget]);
        var newPlan = NewPlan("Monthly");

        var result = await _manager.Register(budget.Id, newPlan, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.BudgetId.Should().Be(budget.Id);
        var plans = await _manager.GetPlans(budget.Id, CancellationToken.None);
        plans.Should().Contain(result.Value);
    }

    [Fact]
    public async Task NotCreatePlanForInaccessibleBudget()
    {
        var result = await _manager.Register(Guid.NewGuid(), NewPlan("Monthly"), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public async Task CopyPlanIntoNewOne()
    {
        var budget = Budget();
        _budgets.Append([budget]);
        var original = (await _manager.Register(budget.Id, NewPlan("Monthly"), CancellationToken.None)).Value;

        var result = await _manager.Copy(budget.Id, original.Id, "Monthly copy", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(original.Id);
        result.Value.Name.Should().Be("Monthly copy");
        result.Value.Expectations.Should().HaveSameCount(original.Expectations);
    }

    [Fact]
    public async Task BuildVarianceReportUsingPlanParameters()
    {
        var budget = Budget();
        _budgets.Append([budget]);
        var tag = new Tag("food");
        var criterion = new UniversalCriterion("All", [new TagBasedCriterion("Food", [tag], TagBasedCriterionType.Including)]);
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        var plan = (await _manager.Register(budget.Id, NewPlan("Monthly", from, till, criterion,
        [
            new PlanExpectation(Guid.NewGuid(), new Money(-100, Currency), from, till, "Food")
        ]), CancellationToken.None)).Value;
        var logbook = new CriteriaBasedLogbook(criterion);
        logbook.Register(Operation(budget, from.AddDays(2), new Money(-80, Currency), tag));
        _reckoner.Setup(r => r.GetLogbook(It.IsAny<LogbookQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logbook);

        var result = await _manager.BuildVarianceReport(budget.Id, plan.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Variances.Single().Expected.Should().Be(new Money(-100, Currency));
        result.Value.Variances.Single().Actual.Should().Be(new Money(-80, Currency));
        _reckoner.Verify(r => r.GetLogbook(
            It.Is<LogbookQuery>(q => q.LogbookCriterion == criterion && q.OutputCurrency == Currency && q.ExcludeTransfers),
            It.IsAny<CancellationToken>()));
    }

    private TrackedBudget Budget()
    {
        return new TrackedBudget(Guid.NewGuid(), "Budget", [_owner], [], [], [LogbookCriteria.Universal])
        {
            Version = Guid.NewGuid().ToString()
        };
    }

    private static UnregisteredBudgetPlan NewPlan(
        string name,
        DateTime? from = null,
        DateTime? till = null,
        Criterion? criterion = null,
        IReadOnlyCollection<PlanExpectation>? expectations = null)
    {
        var planFrom = from ?? new DateTime(2026, 1, 1);
        var planTill = till ?? new DateTime(2026, 2, 1);
        return new UnregisteredBudgetPlan(
            name,
            planFrom,
            planTill,
            null,
            criterion ?? new UniversalCriterion("All"),
            Currency,
            expectations ?? []);
    }

    private static Operation Operation(TrackedBudget budget, DateTime timestamp, Money amount, params Tag[] tags)
    {
        return new Operation(Guid.NewGuid(), timestamp, amount, "Operation", string.Empty, budget, tags, null);
    }
}
