using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Infrastructure.Storage.Repositories;
using NVs.Budget.Infrastructure.Storage.Tests.Fixtures;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Storage.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class TransactionsRepositoryShould : IClassFixture<DbContextManager>, IDisposable
{
    private readonly Fixture _fixture;
    private readonly TransactionsRepository _repo;
    private readonly TestDataFixture _testData;

    public TransactionsRepositoryShould(DbContextManager manager)
    {
        _fixture = manager.TestData.Fixture;
        if (!_fixture.Customizations.Any(c => c is UtcRandomDateTimeSequenceGenerator))
        {
            _fixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());
        }

        _repo = new(manager.Mapper, manager.GetDbBudgetContext(), new VersionGenerator());
        _testData = manager.TestData;
    }

    [Fact]
    public async Task RegisterTransactionSuccessfully()
    {
        var transaction = _fixture.Create<UnregisteredTransaction>();
        var account = _testData.Accounts.First();

        var result = await _repo.Register(transaction, account, CancellationToken.None);
        result.Should().BeSuccess();
        var trackedTransaction = result.Value;

        trackedTransaction.Should().BeEquivalentTo(transaction);
        trackedTransaction.Id.Should().NotBe(Guid.Empty);
        trackedTransaction.Account.Should().BeEquivalentTo(account);
        trackedTransaction.Version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateTransactionSuccessfully()
    {
        var target = await AddTransaction();

        TrackedTransaction updated;
        using (_fixture.SetNamedParameter(nameof(target.Id).ToLower(), target.Id))
        using (_fixture.SetNamedParameter(nameof(target.Account).ToLower(), target.Account))
        using (_fixture.SetNamedParameter(nameof(target.Tags).ToLower(), _fixture.Create<Generator<Tag>>().Take(3)))
        {
            updated = _fixture.Create<TrackedTransaction>();
        }

        updated.Version = target.Version;

        var result = await _repo.Update(updated, CancellationToken.None);
        result.Should().BeSuccess();
        result.Value.Should().BeEquivalentTo(updated);
        result.Value.Version.Should().NotBe(updated.Version);
    }

    private async Task<TrackedTransaction> AddTransaction()
    {
        var unregistered = _fixture.Create<UnregisteredTransaction>();
        var account = _testData.Accounts.First();

        var result = await _repo.Register(unregistered, account, CancellationToken.None);
        result.Should().BeSuccess();
        var target = result.Value!;
        return target;
    }

    [Fact]
    public async Task GetTransactionsThatMatchQuery()
    {
        var key = _fixture.Create<string>();
        var value = _fixture.Create<string>();

        var transaction = await AddTransaction();
        transaction.Attributes[key] = value;

        var result = await _repo.Update(transaction, CancellationToken.None);
        result.Should().BeSuccess();

        var items = await _repo.Get(t => t.Attributes[key].Equals(value), CancellationToken.None);
        items.Should().HaveCount(1);
        items.Single().Should().BeEquivalentTo(transaction, s => s.ComparingByMembers<TrackedTransaction>());
    }

    [Fact]
    public async Task DeleteTransactions()
    {
        var transactions = await AddTransaction();
        var result = await _repo.Remove(transactions, CancellationToken.None);

        var items = await _repo.Get(t => t.Id == transactions.Id, CancellationToken.None);
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
