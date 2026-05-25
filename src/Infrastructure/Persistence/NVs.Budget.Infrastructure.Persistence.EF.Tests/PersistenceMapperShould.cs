using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

public class PersistenceMapperShould
{
    private static readonly Dictionary<Type, Action<Fixture>> Setup = new()
    {
        { typeof(ExchangeRate), SetupCurrenciesForRatesTest },
        { typeof(TrackedTransfer), SetupOperationsForTransfersTest },
        { typeof(LogbookCriteria), SetupLogbookCriteria },
        { typeof(TrackedBudget), SetupLogbookCriteria }
    };

    private readonly Fixture _fixture = new() { Customizations = { new ReadableExpressionsBuilder() } };

    private readonly PersistenceMapper _mapper = new(ReadableExpressionsParser.Default);

    [Theory]
    [InlineData(typeof(Owner), typeof(StoredOwner))]
    [InlineData(typeof(TrackedOwner), typeof(StoredOwner))]
    [InlineData(typeof(TaggingCriterion), typeof(StoredTaggingCriterion))]
    [InlineData(typeof(TransferCriterion), typeof(StoredTransferCriterion))]
    [InlineData(typeof(LogbookCriteria), typeof(StoredLogbookCriteria))]
    [InlineData(typeof(Domain.Entities.Budgets.Budget), typeof(StoredBudget))]
    [InlineData(typeof(TrackedBudget), typeof(StoredBudget))]
    [InlineData(typeof(TrackedOperation), typeof(StoredOperation))]
    [InlineData(typeof(ExchangeRate), typeof(StoredRate))]
    [InlineData(typeof(Money), typeof(StoredMoney))]
    [InlineData(typeof(TrackedTransfer), typeof(StoredTransfer))]
    public void ContainMappingsForDomainEntities(Type sourceType, Type destType)
    {
        if (Setup.TryGetValue(sourceType, out var setup))
        {
            setup(_fixture);
        }

        var instance = _fixture.Create(sourceType, new SpecimenContext(_fixture));
        var dest = MapForward(instance, sourceType, destType);
        var back = MapBackward(dest, destType, sourceType);
        back.Should().BeEquivalentTo(instance);
    }

    private object MapForward(object instance, Type sourceType, Type destType) =>
        (sourceType, destType) switch
        {
            (var s, var d) when s == typeof(Owner) && d == typeof(StoredOwner) => _mapper.ToStored((Owner)instance),
            (var s, var d) when s == typeof(TrackedOwner) && d == typeof(StoredOwner) => _mapper.ToStored((TrackedOwner)instance),
            (var s, var d) when s == typeof(TaggingCriterion) && d == typeof(StoredTaggingCriterion) => _mapper.ToStored((TaggingCriterion)instance),
            (var s, var d) when s == typeof(TransferCriterion) && d == typeof(StoredTransferCriterion) => _mapper.ToStored((TransferCriterion)instance),
            (var s, var d) when s == typeof(LogbookCriteria) && d == typeof(StoredLogbookCriteria) => _mapper.ToStored((LogbookCriteria)instance),
            (var s, var d) when s == typeof(Domain.Entities.Budgets.Budget) && d == typeof(StoredBudget) => _mapper.ToStored((Domain.Entities.Budgets.Budget)instance),
            (var s, var d) when s == typeof(TrackedBudget) && d == typeof(StoredBudget) => _mapper.ToStored((TrackedBudget)instance),
            (var s, var d) when s == typeof(TrackedOperation) && d == typeof(StoredOperation) => _mapper.ToStored((TrackedOperation)instance),
            (var s, var d) when s == typeof(ExchangeRate) && d == typeof(StoredRate) => _mapper.ToStored((ExchangeRate)instance),
            (var s, var d) when s == typeof(Money) && d == typeof(StoredMoney) => _mapper.ToStored((Money)instance),
            (var s, var d) when s == typeof(TrackedTransfer) && d == typeof(StoredTransfer) => _mapper.ToStored((TrackedTransfer)instance),
            _ => throw new ArgumentException($"No forward mapping from {sourceType.Name} to {destType.Name}")
        };

    private object MapBackward(object instance, Type sourceType, Type destType) =>
        (sourceType, destType) switch
        {
            (var s, var d) when s == typeof(StoredOwner) && d == typeof(Owner) => _mapper.ToOwner((StoredOwner)instance),
            (var s, var d) when s == typeof(StoredOwner) && d == typeof(TrackedOwner) => _mapper.ToTrackedOwner((StoredOwner)instance),
            (var s, var d) when s == typeof(StoredTaggingCriterion) && d == typeof(TaggingCriterion) => _mapper.ToTaggingCriterion((StoredTaggingCriterion)instance),
            (var s, var d) when s == typeof(StoredTransferCriterion) && d == typeof(TransferCriterion) => _mapper.ToTransferCriterion((StoredTransferCriterion)instance),
            (var s, var d) when s == typeof(StoredLogbookCriteria) && d == typeof(LogbookCriteria) => _mapper.ToLogbookCriteria((StoredLogbookCriteria)instance),
            (var s, var d) when s == typeof(StoredBudget) && d == typeof(Domain.Entities.Budgets.Budget) => _mapper.ToBudget((StoredBudget)instance),
            (var s, var d) when s == typeof(StoredBudget) && d == typeof(TrackedBudget) => _mapper.ToTrackedBudget((StoredBudget)instance),
            (var s, var d) when s == typeof(StoredOperation) && d == typeof(TrackedOperation) => _mapper.ToTrackedOperation((StoredOperation)instance),
            (var s, var d) when s == typeof(StoredRate) && d == typeof(ExchangeRate) => _mapper.ToExchangeRate((StoredRate)instance)!,
            (var s, var d) when s == typeof(StoredMoney) && d == typeof(Money) => _mapper.ToMoney((StoredMoney)instance),
            (var s, var d) when s == typeof(StoredTransfer) && d == typeof(TrackedTransfer) => _mapper.ToTrackedTransfer((StoredTransfer)instance),
            _ => throw new ArgumentException($"No backward mapping from {sourceType.Name} to {destType.Name}")
        };

    private static void SetupLogbookCriteria(Fixture fixture)
    {
        fixture.Inject((IEnumerable<LogbookCriteria>)
        [
            new LogbookCriteria(
            fixture.Create<string>(),
            [new LogbookCriteria(
                fixture.Create<string>(),
                null,
                fixture.Create<TagBasedCriterionType>(),
                fixture.Create<Generator<Tag>>().Take(5).ToList().AsReadOnly(),
                null, null, null
                    ),
            new LogbookCriteria(
                fixture.Create<string>(),
                null, null, null, fixture.Create<ReadableExpression<Func<Operation, string>>>(),
                null, null
                )],
            null, null, null, null, true
            )
        ]);
    }

    private static void SetupOperationsForTransfersTest(Fixture fixture)
    {
        fixture.Customizations.Add(new TransferOperationsBuilder());
    }

    private static void SetupCurrenciesForRatesTest(Fixture fixture)
    {
        fixture.SetNamedParameter("to", Currency.Xxx);
        fixture.SetNamedParameter("from", Currency.Test);
    }
}
