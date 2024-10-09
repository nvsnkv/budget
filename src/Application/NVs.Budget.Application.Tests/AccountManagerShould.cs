using AutoFixture;
using FluentAssertions;
using Moq;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Application.Tests.Fakes;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

public class AccountManagerShould
{
    private readonly FakeBudgetsRepository _repository = new();
    private readonly Fixture _fixture = new();
    private readonly Mock<IUser> _user = new();

    private readonly BudgetManager _manager;
    private readonly Owner _owner;

    public AccountManagerShould()
    {
        _owner = _fixture.Create<Owner>();
        _user.Setup(u => u.AsOwner()).Returns(_owner);
        _manager = new BudgetManager(_repository, _user.Object);
    }

    [Fact]
    public async Task ReturnOnlyOwnedAccounts()
    {
        var ownedAccounts = GenerateAccounts(3, _owner).ToList();
        var notOwnedAccounts = GenerateAccounts(3, _fixture.Create<Owner>());

        _repository.Append(ownedAccounts);
        _repository.Append(notOwnedAccounts);

        var accounts = await _manager.GetOwnedBudgets(CancellationToken.None);
        accounts.Should().BeEquivalentTo(ownedAccounts);
    }

    [Fact]
    public async Task CreateAccount()
    {
        var newAccount = _fixture.Create<UnregisteredBudget>();
        var result = await _manager.Register(newAccount, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Owners.Should().HaveCount(1);
        result.Value.Owners.Single().Should().Be(_user.Object.AsOwner());

        var accounts = await _manager.GetOwnedBudgets(CancellationToken.None);
        accounts.Should().Contain(result.Value);
    }

    [Fact]
    public async Task RenameOwnedAccounts()
    {
        IReadOnlyList<TrackedBudget> accounts = GenerateAccounts(1, _owner).ToList().AsReadOnly();
        _repository.Append(accounts);

        var expected = new TrackedBudget(accounts[0].Id, accounts[0].Name, accounts[0].Owners)
        {
            Version = accounts[0].Version
        };
        var expectedOwners = expected.Owners.ToList();

        expected.AddOwner(_fixture.Create<Owner>());

        expected.Rename(_fixture.Create<string>());

        var result = await _manager.Update(expected, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();

        accounts = (IReadOnlyList<TrackedBudget>)await _manager.GetOwnedBudgets(CancellationToken.None);
        accounts.Should().Contain(a => a.Id == expected.Id);
        var actual = accounts.Single(a => a.Id == expected.Id);
        actual.Name.Should().Be(expected.Name);
        actual.Owners.Should().BeEquivalentTo(expectedOwners);
    }

    [Fact]
    public async Task NotUpdateAccountThatDoesNotBelongToCurrentOwner()
    {
        var accounts = GenerateAccounts(1, _fixture.Create<Owner>()).ToList();
        _repository.Append(accounts);

        var result = await _manager.Update(accounts.Single(), CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<AccountDoesNotBelongToCurrentOwnerError>();
    }

    [Fact]
    public async Task NotUpdateAccountThatDoesNotExists()
    {
        var result = await _manager.Update(_fixture.Create<TrackedBudget>(), CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<AccountDoesNotExistError>();
    }

    [Fact]
    public async Task ChangeOwnersWhenCurrentOwnerRemainsInList()
    {
        var accounts = GenerateAccounts(1, _owner).ToList();
        _repository.Append(accounts);
        var expected = accounts.Single();

        var owners = _fixture.Create<Generator<Owner>>().Take(3).Append(_owner).ToList();
        var result = await _manager.ChangeOwners(expected, owners, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();

        var actuals = await _manager.GetOwnedBudgets(CancellationToken.None);
        actuals.Should().HaveCount(1);
        var actual = actuals.Single();
        actual.Id.Should().Be(expected.Id);
        actual.Owners.Should().BeEquivalentTo(owners);
    }

    [Fact]
    public async Task NotChangeOwnersIfCurrentOwnerLosesAccess()
    {
        var accounts = GenerateAccounts(1, _fixture.Create<Owner>(), _owner).ToList();
        _repository.Append(accounts);
        var expected = accounts.Single();

        var owners = _fixture.Create<Generator<Owner>>().Take(3).Where(o => o != _owner).ToList();
        var result = await _manager.ChangeOwners(expected, owners, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<CurrentOwnerLosesAccessToAccountError>();
    }

    [Fact]
    public async Task RemoveAccountOwnedOnlyByCurrentOwner()
    {
        IReadOnlyList<TrackedBudget> accounts = GenerateAccounts(1, _owner).ToList().AsReadOnly();
        _repository.Append(accounts);

        var expected = new TrackedBudget(accounts[0].Id, accounts[0].Name, accounts[0].Owners)
        {
            Version = accounts[0].Version
        };

        var result = await _manager.Remove(expected, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NotRemoveAccountOwnedByMultipleOwners()
    {
        var accounts = GenerateAccounts(1, _fixture.Create<Owner>(), _owner).ToList();
        _repository.Append(accounts);

        var result = await _manager.Remove(accounts.Single(), CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<AccountBelongsToMultipleOwnersError>();
    }

    private IEnumerable<TrackedBudget> GenerateAccounts(int count, params Owner[] owners)
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new NamedParameterBuilder<IEnumerable<Owner>>(nameof(owners), owners, false));

        return fixture.Create<Generator<TrackedBudget>>().Take(count);
    }
}
