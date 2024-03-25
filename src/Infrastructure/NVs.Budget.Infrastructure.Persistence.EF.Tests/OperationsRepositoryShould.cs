using AutoFixture;
using FluentAssertions;
using NVs.Budget.Infrastructure.Storage.Tests.Fixtures;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Storage.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class OperationsRepositoryShould : IClassFixture<DbContextManager>, IDisposable
{
    private readonly Fixture _fixture;
    private readonly OperationsRepository _repo;
    private readonly AccountsRepository _accountsRepo;
    private readonly TestDataFixture _testData;

    public OperationsRepositoryShould(DbContextManager manager)
    {
        _fixture = manager.TestData.Fixture;
        if (!_fixture.Customizations.Any(c => c is UtcRandomDateTimeSequenceGenerator))
        {
            _fixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());
        }

        _repo = new(manager.Mapper, manager.GetDbBudgetContext(), new VersionGenerator());
        _accountsRepo = new(manager.Mapper, manager.GetDbBudgetContext(), new VersionGenerator());
        _testData = manager.TestData;
    }

    [Fact]
    public async Task RegisterTransactionSuccessfully()
    {
        var transaction = _fixture.Create<UnregisteredOperation>();
        var accountId = _testData.Accounts.First().Id;
        var accounts = await _accountsRepo.Get(a => a.Id == accountId, CancellationToken.None);
        var account = accounts.Single();

        var result = await _repo.Register(transaction, account, CancellationToken.None);
        result.Should().BeSuccess();
        var trackedTransaction = result.Value;

        trackedTransaction.Should().BeEquivalentTo(transaction, c => c.Excluding(t => t.Account));
        trackedTransaction.Id.Should().NotBe(Guid.Empty);
        trackedTransaction.Account.Should().BeEquivalentTo((Account)account, c => c.ComparingByMembers<Account>());
        trackedTransaction.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateTransactionSuccessfully()
    {
        var target = await AddTransaction();

        TrackedOperation updated;
        using (_fixture.SetNamedParameter(nameof(target.Id).ToLower(), target.Id))
        using (_fixture.SetNamedParameter(nameof(target.Account).ToLower(), target.Account))
        using (_fixture.SetNamedParameter(nameof(target.Tags).ToLower(), _fixture.Create<Generator<Tag>>().Take(3)))
        {
            updated = _fixture.Create<TrackedOperation>();
        }

        updated.Version = target.Version;

        var result = await _repo.Update(updated, CancellationToken.None);
        result.Should().BeSuccess();
        result.Value.Should().BeEquivalentTo(updated);
        result.Value.Version.Should().NotBe(updated.Version);
    }

    private async Task<TrackedOperation> AddTransaction()
    {
        var unregistered = _fixture.Create<UnregisteredOperation>();
        var accountId = _testData.Accounts.First().Id;

        var accounts = await _accountsRepo.Get(a => a.Id == accountId, CancellationToken.None);
        var account = accounts.Single();

        var result = await _repo.Register(unregistered, account, CancellationToken.None);
        result.Should().BeSuccess();
        var target = result.Value!;
        return target;
    }

    [Fact]
    public async Task GetTransactionsThatMatchQuery()
    {
        var key = _fixture.Create<string>().Substring(0,5);
        var value = _fixture.Create<string>();

        var transaction = await AddTransaction();
        _ = await AddTransaction();

        transaction.Attributes[key] = value;

        var result = await _repo.Update(transaction, CancellationToken.None);
        result.Should().BeSuccess();

        var items = await _repo.Get(t => ((string)t.Attributes[key]) == value, CancellationToken.None);
        items.Should().HaveCount(1);
        items.Single().Should().BeEquivalentTo(transaction, s =>
            s.ComparingByMembers<TrackedOperation>()
             .Excluding(o => o.Version)
             .Using<DateTime>(ctx => ctx.Subject.Should().BeOnOrAfter(ctx.Expectation.AddMilliseconds(-500)).And.BeOnOrBefore(ctx.Expectation.AddMilliseconds(500)))
             .WhenTypeIs<DateTime>());
    }

    [Fact]
    public async Task DeleteTransactions()
    {
        var transaction = await AddTransaction();
        _ = await AddTransaction();

        var result = await _repo.Remove(transaction, CancellationToken.None);
        result.Should().BeSuccess();

        var items = await _repo.Get(t => t.Id == transaction.Id, CancellationToken.None);
        items.Should().HaveCount(0);
    }

    public void Dispose()
    {
        var targets = _fixture.Customizations.Where(c => c is UtcRandomDateTimeSequenceGenerator).ToList();
        foreach (var target in targets)
        {
            _fixture.Customizations.Remove(target);
        }
    }
}
