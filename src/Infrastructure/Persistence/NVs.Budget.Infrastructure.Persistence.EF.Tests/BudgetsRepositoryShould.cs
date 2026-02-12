using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
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
            Guid.NewGuid(),
            "Primary",
            manager.TestData.Fixture.Create<string>(),
            [
                new LogbookCriteria(
                    Guid.NewGuid(),
                    "Child 1",
                    manager.TestData.Fixture.Create<string>(),
                    null,
                    manager.TestData.Fixture.Create<TagBasedCriterionType>(),
                    manager.TestData.Fixture.Create<Generator<Tag>>().Take(5).ToList().AsReadOnly(),
                    null, null, null
                ),
                new LogbookCriteria(
                    Guid.NewGuid(),
                    "Child 2",
                    manager.TestData.Fixture.Create<string>(),
                    null, null, null, manager.TestData.Fixture.Create<ReadableExpression<Func<Operation, string>>>(),
                    null, null
                )
            ],
            null, null, null, null, true
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

        result.Value.LogbookCriteria.Should().ContainSingle();
        result.Value.LogbookCriteria.Single().Should().BeEquivalentTo(expected);
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

        var criteria = new LogbookCriteria(Guid.NewGuid(), "Universal", "Universal", [
            new LogbookCriteria(Guid.NewGuid(), "odds", "odds", [
                new LogbookCriteria(Guid.NewGuid(), "subst", "subst", null, null, null, fixture.Create<ReadableExpression<Func<Operation, string>>>(), null, null)
            ], TagBasedCriterionType.Including, [new("Odd")], null, null, null),
            new LogbookCriteria(Guid.NewGuid(), "evens", "evens", null, TagBasedCriterionType.Including, [new("Evens")], null, null, null)
            ],
            null, null, null, null, true
        );

        updated.SetLogbookCriteria(criteria);

        var result = await _repo.Update(updated, CancellationToken.None);
        result.Should().BeSuccess();
        var red = (await _repo.Get(a => a.Id == id, CancellationToken.None)).Single();
        red.LogbookCriteria.Should().ContainSingle();
        red.LogbookCriteria.Single().Should().BeEquivalentTo(criteria);
    }

    [Fact]
    public async Task UpdateSingleLogbookWithoutAffectingOtherLogbooks()
    {
        var id = manager.TestData.Budgets.First().Id;
        var targets = await _repo.Get(a => a.Id == id, CancellationToken.None);
        var target = targets.Single();

        var primary = new LogbookCriteria(Guid.NewGuid(), "Primary", "Primary criteria", null, null, null, null, null, true);
        var secondary = new LogbookCriteria(Guid.NewGuid(), "Secondary", "Secondary criteria", null, null, null, null, null, true);

        var seed = new TrackedBudget(target.Id, target.Name, target.Owners, target.TaggingCriteria, target.TransferCriteria, [primary, secondary])
        {
            Version = target.Version
        };
        var seedResult = await _repo.Update(seed, CancellationToken.None);
        seedResult.Should().BeSuccess();

        var updatedPrimary = new LogbookCriteria(primary.CriteriaId, primary.Name, "Updated primary", null, null, null, null, null, true);

        var update = new TrackedBudget(target.Id, target.Name, target.Owners, target.TaggingCriteria, target.TransferCriteria, [updatedPrimary, secondary])
        {
            Version = seedResult.Value.Version
        };

        var updateResult = await _repo.Update(update, CancellationToken.None);
        updateResult.Should().BeSuccess();

        var reloaded = (await _repo.Get(a => a.Id == id, CancellationToken.None)).Single();
        reloaded.LogbookCriteria.Should().HaveCount(2);
        reloaded.LogbookCriteria.Single(l => l.CriteriaId == updatedPrimary.CriteriaId).Description.Should().Be("Updated primary");
        reloaded.LogbookCriteria.Single(l => l.CriteriaId == secondary.CriteriaId).Description.Should().Be("Secondary criteria");
    }

}
