using AutoFixture;
using FluentAssertions;
using Moq;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Services.Accounting;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Application.Tests.Fakes;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

public class BudgetManagerShould
{
    private readonly FakeBudgetsRepository _repository = new();
    private readonly Fixture _fixture = new();
    private readonly Mock<IUser> _user = new();

    private readonly BudgetManager _manager;
    private readonly Owner _owner;

    public BudgetManagerShould()
    {
        _fixture.Customizations.Add(new ReadableExpressionsBuilder());
        _owner = _fixture.Create<Owner>();
        _user.Setup(u => u.AsOwner()).Returns(_owner);
        _manager = new BudgetManager(_repository, _user.Object);
    }

    [Fact]
    public async Task ReturnOnlyOwnedAccounts()
    {
        var ownedAccounts = GenerateBudgets(3, _owner).ToList();
        var notOwnedAccounts = GenerateBudgets(3, _fixture.Create<Owner>());

        _repository.Append(ownedAccounts);
        _repository.Append(notOwnedAccounts);

        var budgets = await _manager.GetOwnedBudgets(CancellationToken.None);
        budgets.Should().BeEquivalentTo(ownedAccounts);
    }

    [Fact]
    public async Task CreateAccount()
    {
        var newAccount = _fixture.Create<UnregisteredBudget>();
        var result = await _manager.Register(newAccount, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Owners.Should().HaveCount(1);
        result.Value.Owners.Single().Should().Be(_user.Object.AsOwner());

        var budgets = await _manager.GetOwnedBudgets(CancellationToken.None);
        budgets.Should().Contain(result.Value);
    }

    [Fact]
    public async Task RenameOwnedAccounts()
    {
        IReadOnlyList<TrackedBudget> getOwnedBudgets = GenerateBudgets(1, _owner).ToList().AsReadOnly();
        _repository.Append(getOwnedBudgets);

        var expected = new TrackedBudget(getOwnedBudgets[0].Id, getOwnedBudgets[0].Name, getOwnedBudgets[0].Owners, getOwnedBudgets[0].TaggingCriteria, getOwnedBudgets[0].TransferCriteria)
        {
            Version = getOwnedBudgets[0].Version
        };
        var expectedOwners = expected.Owners.ToList();

        expected.AddOwner(_fixture.Create<Owner>());

        expected.Rename(_fixture.Create<string>());

        var result = await _manager.Update(expected, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();

        getOwnedBudgets = (IReadOnlyList<TrackedBudget>)await _manager.GetOwnedBudgets(CancellationToken.None);
        getOwnedBudgets.Should().Contain(a => a.Id == expected.Id);
        var actual = getOwnedBudgets.Single(a => a.Id == expected.Id);
        actual.Name.Should().Be(expected.Name);
        actual.Owners.Should().BeEquivalentTo(expectedOwners);
    }

    [Fact]
    public async Task NotUpdateAccountThatDoesNotBelongToCurrentOwner()
    {
        var budgets = GenerateBudgets(1, _fixture.Create<Owner>()).ToList();
        _repository.Append(budgets);

        var result = await _manager.Update(budgets.Single(), CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<BudgetDoesNotBelongToCurrentOwnerError>();
    }

    [Fact]
    public async Task NotUpdateAccountThatDoesNotExists()
    {
        var result = await _manager.Update(_fixture.Create<TrackedBudget>(), CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<BudgetDoesNotExistError>();
    }

    [Fact]
    public async Task ChangeOwnersWhenCurrentOwnerRemainsInList()
    {
        var budgets = GenerateBudgets(1, _owner).ToList();
        _repository.Append(budgets);
        var expected = budgets.Single();

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
        var budgets = GenerateBudgets(1, _fixture.Create<Owner>(), _owner).ToList();
        _repository.Append(budgets);
        var expected = budgets.Single();

        var owners = _fixture.Create<Generator<Owner>>().Take(3).Where(o => o != _owner).ToList();
        var result = await _manager.ChangeOwners(expected, owners, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<CurrentOwnerLosesAccessToBudgetError>();
    }

    [Fact]
    public async Task RemoveAccountOwnedOnlyByCurrentOwner()
    {
        IReadOnlyList<TrackedBudget> budgets = GenerateBudgets(1, _owner).ToList().AsReadOnly();
        _repository.Append(budgets);

        var expected = new TrackedBudget(budgets[0].Id, budgets[0].Name, budgets[0].Owners, budgets[0].TaggingCriteria, budgets[0].TransferCriteria)
        {
            Version = budgets[0].Version
        };

        var result = await _manager.Remove(expected, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task NotRemoveAccountOwnedByMultipleOwners()
    {
        var budgets = GenerateBudgets(1, _fixture.Create<Owner>(), _owner).ToList();
        _repository.Append(budgets);

        var result = await _manager.Remove(budgets.Single(), CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<BudgetBelongsToMultipleOwnersError>();
    }

    private IEnumerable<TrackedBudget> GenerateBudgets(int count, params Owner[] owners)
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new NamedParameterBuilder<IEnumerable<Owner>>(nameof(owners), owners, false));
        fixture.Customizations.Add(new ReadableExpressionsBuilder());

        return fixture.Create<Generator<TrackedBudget>>().Take(count);
    }
}
