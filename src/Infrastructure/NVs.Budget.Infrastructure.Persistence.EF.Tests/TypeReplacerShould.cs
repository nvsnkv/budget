using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

public class TypeReplacerShould
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ReplaceAccountSuccessfully()
    {
        Expression<Func<TrackedAccount, bool>> selectById = a => a.Id != Guid.Empty;
        var converted = selectById.ConvertTypes<TrackedAccount, StoredAccount>(MappingProfile.TypeMappings);
        converted.Should().NotBeNull();

        var account = _fixture.Build<StoredAccount>()
            .Without(a => a.Owners)
            .Without(a => a.Operations)
            .Create();

        var predicate = converted.Compile();
        predicate(account).Should().BeTrue();
    }

    [Fact]
    public void PreserveClosures()
    {
        var reference = _fixture.Create<TrackedAccount>();
        Expression<Func<TrackedAccount, bool>> expression = a => a.Name == reference.Name && a.Bank == reference.Bank;

        var converted = expression.ConvertTypes<TrackedAccount, StoredAccount>(MappingProfile.TypeMappings);
        converted.Should().NotBeNull();

        var predicate = converted.Compile();
        var account = new StoredAccount(reference.Id, reference.Name, reference.Bank);
        predicate(account).Should().BeTrue();
    }

    [Fact]
    public void HandleNestedTypes()
    {
        var owner = _fixture.Create<Owner>();
        Expression<Func<TrackedAccount, bool>> forOwner = a => a.Owners.Any(o => o.Id == owner.Id);
        var action = () => forOwner.ConvertTypes<TrackedAccount, StoredAccount>(MappingProfile.TypeMappings);
        action.Should().NotThrow();
    }

    [Fact]
    public void HandleCollectionsThatDoesNotRequireConversion()
    {
        var owners = _fixture.Create<Generator<Owner>>().Take(3).Select(t=> t.Id).ToList();
        Expression<Func<TrackedAccount, bool>> forOwners = a => a.Owners.Any(o => owners.Contains(o.Id));
        var action = () => forOwners.ConvertTypes<TrackedAccount, StoredAccount>(MappingProfile.TypeMappings);
        action.Should().NotThrow();
    }

    [Fact]
    public void ReplaceTypesInMembers()
    {
        var id = _fixture.Create<Guid>();

        Expression<Func<TrackedOperation, bool>> availableAccounts = o => o.Account.Id == id;
        var action = () => availableAccounts.ConvertTypes<TrackedOperation, StoredOperation>(MappingProfile.TypeMappings);
        action.Should().NotThrow();
    }
}
