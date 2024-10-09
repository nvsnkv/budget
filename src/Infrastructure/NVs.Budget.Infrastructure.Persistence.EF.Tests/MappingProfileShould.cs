using AutoFixture;
using AutoFixture.Kernel;
using AutoMapper;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

public class MappingProfileShould
{
    private static readonly Dictionary<Type, Action<Fixture>> Setup = new()
    {
        { typeof(ExchangeRate), SetupCurrenciesForRatesTest },
        { typeof(TrackedTransfer), SetupOperationsForTransfersTest }
    };

    private readonly Fixture _fixture = new();

    private readonly Mapper _mapper = new(new MapperConfiguration(config => config.AddProfile(new MappingProfile())));

    [Theory]
    [InlineData(typeof(Owner), typeof(StoredOwner))]
    [InlineData(typeof(TrackedOwner), typeof(StoredOwner))]
    [InlineData(typeof(Domain.Entities.Accounts.Budget), typeof(StoredAccount))]
    [InlineData(typeof(TrackedBudget), typeof(StoredAccount))]
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
