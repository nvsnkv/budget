using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;
using NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class AccountsRepositoryShould(DbContextManager manager): IClassFixture<DbContextManager>
{
    private readonly AccountsRepository _repo = new(manager.Mapper, manager.GetDbBudgetContext(), new VersionGenerator());

    [Fact]
    public async Task RegisterAnAccount()
    {
        var owner = manager.TestData.Owners.First();

        var account = manager.TestData.Fixture.Create<UnregisteredAccount>();
        var result = await _repo.Register(account, owner, CancellationToken.None);

        result.Should().BeSuccess();

        var created = result.Value;
        created.Should().NotBeNull().And.BeEquivalentTo(account);
        created.Id.Should().NotBe(default(Guid));
        created.Version.Should().NotBeNullOrEmpty();

        var red = await _repo.Get(a => a.Id == created.Id, CancellationToken.None);
        red.Should().HaveCount(1);
        red.Single().Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GetAccount()
    {
        var expected = manager.TestData.Accounts.Last();
        var collection = await _repo.Get(a => a.Id == expected.Id, CancellationToken.None);
        collection.Should().HaveCount(1);
        collection.Single().Should().BeEquivalentTo(expected, c => c.ComparingByMembers<TrackedBudget>());
    }

    [Fact]
    public async Task UpdateAccountIfVersionsAreTheSame()
    {
        var id = manager.TestData.Accounts.First().Id;
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
    public async Task NotUpdateAccountIfVersionsAreDifferent()
    {
        var target = manager.TestData.Accounts.First();
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

}
