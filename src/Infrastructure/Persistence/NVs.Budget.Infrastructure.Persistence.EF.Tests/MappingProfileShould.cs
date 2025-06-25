using AutoFixture;
using AutoFixture.Kernel;
using AutoMapper;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;
using NVs.Budget.Utilities.Expressions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

public class MappingProfileShould
{
    private static readonly Dictionary<Type, Action<Fixture>> Setup = new()
    {
        { typeof(ExchangeRate), SetupCurrenciesForRatesTest },
        { typeof(TrackedTransfer), SetupOperationsForTransfersTest },
        { typeof(LogbookCriteria), SetupLogbookCriteria },
        { typeof(TrackedBudget), SetupLogbookCriteria }
    };

    private readonly Fixture _fixture = new() { Customizations = { new ReadableExpressionsBuilder() }};

    private readonly Mapper _mapper = new(new MapperConfiguration(config => config.AddProfile(new MappingProfile(ReadableExpressionsParser.Default))));

    [Theory]
    [InlineData(typeof(Owner), typeof(StoredOwner))]
    [InlineData(typeof(TrackedOwner), typeof(StoredOwner))]
    [InlineData(typeof(TaggingCriterion), typeof(StoredTaggingCriterion))]
    [InlineData(typeof(TransferCriterion), typeof(StoredTransferCriterion))]
    [InlineData(typeof(LogbookCriteria), typeof(StoredLogbookCriteria))]
    [InlineData(typeof(Domain.Entities.Accounts.Budget), typeof(StoredBudget))]
    [InlineData(typeof(TrackedBudget), typeof(StoredBudget))]
    [InlineData(typeof(TrackedOperation), typeof(StoredOperation))]
    [InlineData(typeof(ExchangeRate), typeof(StoredRate))]
    [InlineData(typeof(Money), typeof(StoredMoney))]
    [InlineData(typeof(TrackedTransfer), typeof(StoredTransfer))]
    public void ContainMappingsForDomainEntities(Type sourceType, Type destType)
    {
        if (Setup.TryGetValue(sourceType, out var setup))
        {
            setup(_fixture);}

        var instance = _fixture.Create(sourceType, new SpecimenContext(_fixture));
        var dest = _mapper.Map(instance, sourceType, destType);
        var back = _mapper.Map(dest, destType, sourceType);
        back.Should().BeEquivalentTo(instance);
    }

    private static void SetupLogbookCriteria(Fixture fixture)
    {
        fixture.Inject(new LogbookCriteria(
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
            ));
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
