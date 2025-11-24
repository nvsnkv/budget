using System.Linq.Expressions;
using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

public class TypeReplacerShould
{
    private readonly Fixture _fixture = new() { Customizations = { new ReadableExpressionsBuilder() }};

    public TypeReplacerShould()
    {
        _fixture.Inject(LogbookCriteria.Universal);
    }

    [Fact]
    public void ReplaceBudgetSuccessfully()
    {
        Expression<Func<TrackedBudget, bool>> selectById = a => a.Id != Guid.Empty;
        var converted = selectById.ConvertTypes<TrackedBudget, StoredBudget>(MappingProfile.TypeMappings);
        converted.Should().NotBeNull();

        var budget = _fixture.Build<StoredBudget>()
            .Without(b => b.Owners)
            .Without(b => b.Operations)
            .Without(b => b.CsvReadingOptions)
            .Without(b => b.TaggingCriteria)
            .Without(b => b.TransferCriteria)
            .Without(b => b.LogbookCriteria)
            .Create();

        var predicate = converted.Compile();
        predicate(budget).Should().BeTrue();
    }

    [Fact]
    public void PreserveClosures()
    {
        var reference = _fixture.Create<TrackedBudget>();
        Expression<Func<TrackedBudget, bool>> expression = a => a.Name == reference.Name;

        var converted = expression.ConvertTypes<TrackedBudget, StoredBudget>(MappingProfile.TypeMappings);
        converted.Should().NotBeNull();

        var predicate = converted.Compile();
        var budget = new StoredBudget(reference.Id, reference.Name);
        predicate(budget).Should().BeTrue();
    }

    [Fact]
    public void HandleNestedTypes()
    {
        var owner = _fixture.Create<Owner>();
        Expression<Func<TrackedBudget, bool>> forOwner = a => a.Owners.Any(o => o.Id == owner.Id);
        var action = () => forOwner.ConvertTypes<TrackedBudget, StoredBudget>(MappingProfile.TypeMappings);
        action.Should().NotThrow();
    }

    [Fact]
    public void HandleCollectionsThatDoesNotRequireConversion()
    {
        var owners = _fixture.Create<Generator<Owner>>().Take(3).Select(t=> t.Id).ToList();
        Expression<Func<TrackedBudget, bool>> forOwners = a => a.Owners.Any(o => owners.Contains(o.Id));
        var action = () => forOwners.ConvertTypes<TrackedBudget, StoredBudget>(MappingProfile.TypeMappings);
        action.Should().NotThrow();
    }

    [Fact]
    public void ReplaceTypesInMembers()
    {
        var id = _fixture.Create<Guid>();

        Expression<Func<TrackedOperation, bool>> availableBudgets = o => o.Budget.Id == id;
        var action = () => availableBudgets.ConvertTypes<TrackedOperation, StoredOperation>(MappingProfile.TypeMappings);
        action.Should().NotThrow();
    }

    [Fact]
    public void ReplaceNestedTypesWithinExtensionMethods()
    {
        var owner = _fixture.Create<Owner>();
        var newBudget = _fixture.Create<UnregisteredBudget>();
        Expression<Func<TrackedBudget, bool>> expression = a => a.Owners.Any(o => o.Id == owner.Id) && a.Name == newBudget.Name;

        var action = () => expression.ConvertTypes<TrackedBudget, StoredBudget>(MappingProfile.TypeMappings);
        action.Should().NotThrow();
    }
}
