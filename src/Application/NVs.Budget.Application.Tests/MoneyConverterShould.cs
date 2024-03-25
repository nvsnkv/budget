using AutoFixture;
using FluentAssertions;
using Moq;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Services.Accounting.Exchange;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

public class MoneyConverterShould
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IExchangeRatesRepository> _repository = new();
    private readonly Mock<IExchangeRatesProvider> _provider = new();
    private readonly Owner _owner;
    private readonly Mock<IUser> _user;

    public MoneyConverterShould()
    {
        _owner = _fixture.Create<Owner>();
        _user = new Mock<IUser>();
        _user.Setup(u => u.AsOwner()).Returns(_owner);
    }


    [Fact]
    public async Task NotChangeTransactionIfCurrenciesAreTheSame()
    {
        var transaction = _fixture.Create<Operation>();

        var converter = new MoneyConverter(_repository.Object, _provider.Object, _user.Object);
        var notConverted = await converter.Convert(transaction, transaction.Amount.GetCurrency(), CancellationToken.None);
        notConverted.Should().Be(transaction);
    }

    [Fact]
    public async Task UseStoredExchangeRateFirst()
    {
        _fixture.SetCurrency(CurrencyIsoCode.MYR);
        var transaction = _fixture.Create<Operation>();
        var targetCurrency = Currency.Dkk;
        var rate = 4m;

        _repository.Setup(r => r.GetRate(_owner, transaction.Timestamp, transaction.Amount.GetCurrency(), targetCurrency, It.IsAny<CancellationToken>()))
            .ReturnsAsync( new ExchangeRate(transaction.Timestamp, transaction.Amount.GetCurrency(), targetCurrency, rate));

        var converter = new MoneyConverter(_repository.Object, _provider.Object, _user.Object);

        var converted = await converter.Convert(transaction, targetCurrency, CancellationToken.None);

        converted.Amount.GetCurrency().Should().Be(targetCurrency);
        converted.Amount.Amount.Should().Be(transaction.Amount.Amount * rate);
        converted.Should().BeEquivalentTo(transaction, o => o.Excluding(t => t.Amount));
    }

    [Fact]
    public async Task UseExternalProviderWhenNoRatesFound()
    {
        _fixture.SetCurrency(CurrencyIsoCode.MYR);
        var transaction = _fixture.Create<Operation>();
        var targetCurrency = Currency.Dkk;
        var rate = 4m;

        var expectedRate = new ExchangeRate(transaction.Timestamp, transaction.Amount.GetCurrency(), targetCurrency, rate);
        _provider
            .Setup(p => p.Get(transaction.Timestamp, transaction.Amount.GetCurrency(), targetCurrency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRate);

        _repository
            .Setup(r => r.Add(expectedRate, _owner, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var converter = new MoneyConverter(_repository.Object, _provider.Object, _user.Object);

        var converted = await converter.Convert(transaction, targetCurrency, CancellationToken.None);

        converted.Amount.GetCurrency().Should().Be(targetCurrency);
        converted.Amount.Amount.Should().Be(transaction.Amount.Amount * rate);
        converted.Should().BeEquivalentTo(transaction, o => o.Excluding(t => t.Amount));
        _repository.VerifyAll();
    }


}
