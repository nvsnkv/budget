using AutoFixture;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;
using NVs.Budget.Infrastructure.Storage.Tests.Fixtures;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Storage.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class ExchangeRatesRepositoryShould: IClassFixture<DbContextManager>, IAsyncLifetime
{
    private readonly DbContextManager _manager;
    private readonly ExchangeRatesRepository? _repo;
    private Owner? _owner;

    public ExchangeRatesRepositoryShould(DbContextManager manager)
    {
        _manager = manager;
        _repo = new(manager.GetDbBudgetContext(), _manager.Mapper);
    }

    [Fact]
    public async Task AddRateSuccessfully()
    {
        var rate = _manager.TestData.Fixture.Create<ExchangeRate>();
        await DoAddRateForOwner(rate);

        var stored = await _repo!.GetRate(_owner!, rate.AsOf, rate.From, rate.To, CancellationToken.None);
        stored.Should().NotBeNull().And.BeEquivalentTo(rate, s => s.ComparingByMembers<ExchangeRate>());
    }

    [Fact]
    public async Task NotReturnRateStoredForDifferentOwner()
    {
        var rate = _manager.TestData.Fixture.Create<ExchangeRate>();
        await DoAddRateForOwner(rate);

        var anotherOwner = _manager.TestData.Fixture.Create<Owner>();
        var stored = await _repo!.GetRate(anotherOwner, rate.AsOf, rate.From, rate.To, CancellationToken.None);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task ReturnRateWithGreatestAsOfThatLessOrEqualToRequested()
    {
        var fixture = _manager.TestData.Fixture;
        fixture.Customizations.Add(new StrictlyMonotonicallyIncreasingDateTimeGenerator(fixture.Create<DateTime>()));


        await DoAddRateForOwner(fixture.Create<ExchangeRate>());
        await DoAddRateForOwner(fixture.Create<ExchangeRate>());
        await DoAddRateForOwner(fixture.Create<ExchangeRate>());

        var closest = fixture.Create<ExchangeRate>();
        await DoAddRateForOwner(closest);

        var targets = fixture.Customizations.Where(c => c is StrictlyMonotonicallyIncreasingDateTimeGenerator).ToList();
        foreach (var target in targets)
        {
            fixture.Customizations.Remove(target);
        }

        var stored = await _repo!.GetRate(_owner!, closest.AsOf.AddSeconds(15), closest.From, closest.To, CancellationToken.None);
        stored.Should().NotBeNull().And.BeEquivalentTo(closest, c => c.ComparingByMembers<ExchangeRate>());
    }

    private async Task DoAddRateForOwner(ExchangeRate rate)
    {
        _repo.Should().NotBeNull();
        _owner.Should().NotBeNull();

        await _repo!.Add(rate, _owner!, CancellationToken.None);
    }

    public async Task InitializeAsync()
    {
        var fixture = _manager.TestData.Fixture;
        if (!fixture.Customizations.Any(c => c is UtcRandomDateTimeSequenceGenerator))
        {
            fixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());
        }

        fixture.SetNamedParameter(nameof(ExchangeRate.From).ToLower(), Currency.Xxx);
        fixture.SetNamedParameter(nameof(ExchangeRate.To).ToLower(), Currency.Test);

        _owner = _manager.TestData.Fixture.Create<Owner>();
        await using var context = _manager.GetDbBudgetContext();
        await context.Owners.AddAsync(_manager.Mapper.Map<StoredOwner>(_owner));
        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        var fixture = _manager.TestData.Fixture;
        var targets = fixture.Customizations.Where(c => c is UtcRandomDateTimeSequenceGenerator).ToList();
        foreach (var target in targets)
        {
            fixture.Customizations.Remove(target);
        }

        fixture.ResetNamedParameter<Currency>(nameof(ExchangeRate.From).ToLower());
        fixture.ResetNamedParameter<Currency>(nameof(ExchangeRate.To).ToLower());


        return Task.CompletedTask;
    }
}
