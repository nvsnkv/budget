using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;
using NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class BudgetsRepositoryShould(DbContextManager manager): IClassFixture<DbContextManager>
{
    private readonly BudgetsRepository _repo = new(manager.Mapper, manager.GetDbBudgetContext(), new VersionGenerator());

    [Fact]
    public async Task RegisterAnBudget()
    {
        var owner = manager.TestData.Owners.First();

        var budget = manager.TestData.Fixture.Create<UnregisteredBudget>();
        var result = await _repo.Register(budget, owner, CancellationToken.None);

        result.Should().BeSuccess();

        var created = result.Value;
        created.Should().NotBeNull().And.BeEquivalentTo(budget);
        created.Id.Should().NotBe(default(Guid));
        created.Version.Should().NotBeNullOrEmpty();

        var red = await _repo.Get(a => a.Id == created.Id, CancellationToken.None);
        red.Should().HaveCount(1);
        red.Single().Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GetBudget()
    {
        var expected = manager.TestData.Budgets.Last();
        var collection = await _repo.Get(a => a.Id == expected.Id, CancellationToken.None);
        collection.Should().HaveCount(1);
        collection.Single().Should().BeEquivalentTo(expected, c => c.ComparingByMembers<TrackedBudget>());
    }

    [Fact]
    public async Task UpdateLogbookCriteriaForBudget()
    {
        var expected = new LogbookCriteria(
            manager.TestData.Fixture.Create<string>(),
            [
                new LogbookCriteria(
                    manager.TestData.Fixture.Create<string>(),
                    null,
                    manager.TestData.Fixture.Create<TagBasedCriterionType>(),
                    manager.TestData.Fixture.Create<Generator<Tag>>().Take(5).ToList().AsReadOnly(),
                    null, null
                ),
                new LogbookCriteria(
                    manager.TestData.Fixture.Create<string>(),
                    null, null, null, manager.TestData.Fixture.Create<ReadableExpression<Func<Operation, string>>>(),
                    null
                )
            ],
            null, null, null, null
        );

        var id = manager.TestData.Budgets.First().Id;
        var targets = await _repo.Get(b => b.Id == id, CancellationToken.None);
        var target = targets.Single();
        var updated = new TrackedBudget(target.Id, target.Name, target.Owners, target.TaggingCriteria, target.TransferCriteria, expected)
        {
            Version = target.Version
        };

        var result = await _repo.Update(updated, CancellationToken.None);
        result.Should().BeSuccess();

        result.Value.LogbookCriteria.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task UpdateBudgetIfVersionsAreTheSame()
    {
        var id = manager.TestData.Budgets.First().Id;
        var targets = await _repo.Get(a => a.Id == id, CancellationToken.None);
        var target = targets.Single();

        var fixture = manager.TestData.Fixture;
        TrackedBudget updated;
        using(fixture.SetNamedParameter(nameof(target.Id).ToLower(), target.Id))
        using (fixture.SetNamedParameter(nameof(target.Owners).ToLower(), manager.TestData.Owners.AsEnumerable()))
        {
            updated = fixture.Create<TrackedBudget>();
        }

        updated.Version = target.Version;

        var result = await _repo.Update(updated, CancellationToken.None);
        result.Should().BeSuccess();
        result.Value.Should().BeEquivalentTo(
            updated,
            c => c.ComparingByMembers<TrackedBudget>()
                .Excluding(a => a.Version)
        );
        result.Value.Version.Should().NotBeNull().And.NotBe(target.Version);
    }

    [Fact]
    public async Task NotUpdateBudgetIfVersionsAreDifferent()
    {
        var target = manager.TestData.Budgets.First();
        var fixture = manager.TestData.Fixture;
        TrackedBudget updated;
        using(fixture.SetNamedParameter(nameof(target.Id).ToLower(), target.Id))
        using (fixture.SetNamedParameter(nameof(target.Owners).ToLower(), manager.TestData.Owners.AsEnumerable()))
        {
            updated = fixture.Create<TrackedBudget>();
        }

        updated.Version = fixture.Create<string>();

        var result = await _repo.Update(updated, CancellationToken.None);
        result.Should().BeFailure();
        result.Should().HaveReason<VersionDoesNotMatchError<TrackedBudget, Guid>>("Version of entity differs from recorded entity!");
    }

    [Fact]
    public async Task UpdateLogbookCriteriaWithChildren()
    {
        var id = manager.TestData.Budgets.First().Id;
        var targets = await _repo.Get(a => a.Id == id, CancellationToken.None);
        var target = targets.Single();

        var fixture = manager.TestData.Fixture;
        TrackedBudget updated;
        using(fixture.SetNamedParameter(nameof(target.Id).ToLower(), target.Id))
        using (fixture.SetNamedParameter(nameof(target.Owners).ToLower(), manager.TestData.Owners.AsEnumerable()))
        {
            updated = fixture.Create<TrackedBudget>();
        }

        updated.Version = target.Version;

        var criteria = new LogbookCriteria("Universal", [
            new LogbookCriteria("odds", [
                new LogbookCriteria("subst", null, null, null, fixture.Create<ReadableExpression<Func<Operation, string>>>(), null)
            ], TagBasedCriterionType.Including, [new("Odd")], null, null),
            new LogbookCriteria("evens", null, TagBasedCriterionType.Including, [new("Evens")], null, null)
            ],
            null, null, null, null
        );

        updated.SetLogbookCriteria(criteria);

        var result = await _repo.Update(updated, CancellationToken.None);
        result.Should().BeSuccess();
        var red = (await _repo.Get(a => a.Id == id, CancellationToken.None)).Single();
        red.LogbookCriteria.Should().BeEquivalentTo(criteria);
    }

}
