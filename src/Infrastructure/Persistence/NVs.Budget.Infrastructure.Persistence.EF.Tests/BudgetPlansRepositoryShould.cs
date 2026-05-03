using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Planning;
using NVs.Budget.Domain.Entities.Plans;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;
using NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class BudgetPlansRepositoryShould(DbContextManager manager)
{
    private static readonly Currency Currency = Currency.Get(CurrencyIsoCode.RUB);
    private readonly BudgetPlansRepository _repo = new(manager.Mapper, manager.GetDbBudgetContext(), new VersionGenerator());

    [Fact]
    public async Task RegisterAndReadPlanWithJsonExpectations()
    {
        var budget = manager.TestData.Budgets.First();
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        var criteria = new LogbookCriteria(
            "Food",
            null,
            TagBasedCriterionType.Including,
            [new Tag("food")],
            null,
            null,
            null);
        var newPlan = new UnregisteredBudgetPlan(
            "January",
            from,
            till,
            null,
            criteria.GetCriterion(),
            Currency,
            [new PlanExpectation(Guid.NewGuid(), new Money(-100, Currency), from, till, "Food", "Groceries")],
            null,
            null,
            criteria);

        var result = await _repo.Register(newPlan, budget, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Version.Should().NotBeNullOrEmpty();

        var loaded = (await _repo.Get(p => p.Id == result.Value.Id, CancellationToken.None)).Single();
        loaded.Name.Should().Be("January");
        loaded.BudgetId.Should().Be(budget.Id);
        loaded.LogbookCriteria.Should().NotBeNull();
        loaded.Expectations.Single().ExpectedAmount.Should().Be(new Money(-100, Currency));
        loaded.Expectations.Single().SubcriterionName.Should().Be("Food");
    }

    [Fact]
    public async Task UpdatePlanAndBumpVersion()
    {
        var budget = manager.TestData.Budgets.First();
        var created = (await _repo.Register(NewPlan("Original", budget.Id), budget, CancellationToken.None)).Value;
        var updated = new TrackedBudgetPlan(
            created.Id,
            created.BudgetId,
            "Updated",
            created.From,
            created.Till,
            created.CronExpression,
            created.Criterion,
            created.Currency,
            created.Expectations,
            created.ExpectedAmount,
            created.Note,
            created.LogbookCriteria)
        {
            Version = created.Version
        };

        var result = await _repo.Update(updated, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Name.Should().Be("Updated");
        result.Value.Version.Should().NotBe(created.Version);
    }

    private static UnregisteredBudgetPlan NewPlan(string name, Guid budgetId)
    {
        var from = new DateTime(2026, 1, 1);
        var till = new DateTime(2026, 2, 1);
        var criteria = LogbookCriteria.Universal;
        return new UnregisteredBudgetPlan(
            name,
            from,
            till,
            null,
            criteria.GetCriterion(),
            Currency,
            [new PlanExpectation(Guid.NewGuid(), new Money(10, Currency), from, till)],
            null,
            $"Budget {budgetId}",
            criteria);
    }
}
