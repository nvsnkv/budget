using AutoFixture;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Domain.Tests;

public class TransferShould
{
    [Fact]
    public void CalculateFeeForTransactionsInSameCurrency()
    {
        var currencyIsoCode = new Fixture().Create<CurrencyIsoCode>();
        var source = CreateTransaction(-1000, -1, currencyIsoCode);
        var sink = CreateTransaction(1, 1000, currencyIsoCode);

        var diff = sink.Amount + source.Amount;

        var transfer = new Transfer(source, sink, new Fixture().Create<string>());
        transfer.Fee.Should().Be(diff);
    }

    [Fact]
    public void CreateTransactionForSinkAccountIfFeeIsPositive()
    {
        var currencyIsoCode = new Fixture().Create<CurrencyIsoCode>();
        var source = CreateTransaction(-100, -1, currencyIsoCode);
        var sink = CreateTransaction(101, 1000, currencyIsoCode);

        var transfer = new Transfer(source, sink, new Fixture().Create<string>());
        var transaction = transfer.AsTransaction();

        transaction.Amount.Should().Be(transfer.Fee);
        transaction.Account.Should().Be(sink.Account);
    }

    [Fact]
    public void CreateTransactionForSourceAccountIfFeeIsNegative()
    {
        var currencyIsoCode = new Fixture().Create<CurrencyIsoCode>();
        var source = CreateTransaction(-1000, -100, currencyIsoCode);
        var sink = CreateTransaction(19, 99, currencyIsoCode);

        var transfer = new Transfer(source, sink, new Fixture().Create<string>());
        var transaction = transfer.AsTransaction();

        transaction.Amount.Should().Be(transfer.Fee);
        transaction.Account.Should().Be(source.Account);
    }

    [Fact]
    public void CalculateZeroFeeProperly()
    {
        var fixture = new Fixture();
        var source = CreateTransaction(-100, -10, fixture.Create<CurrencyIsoCode>());
        fixture.Customizations.Add(new NamedParameterBuilder<Money>("amount", source.Amount * -1, false));
        var sink = fixture.Create<Transaction>();

        var transfer = new Transfer(source, sink, fixture.Create<string>());
        transfer.Fee.IsZero().Should().BeTrue();
    }

    private Transaction CreateTransaction(int minAmount, int maxAmount, CurrencyIsoCode currency)
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new RandomNumericSequenceGenerator(minAmount, maxAmount));
        fixture.Customizations.Add(new NamedParameterBuilder<CurrencyIsoCode>("currency", currency, false));

        return fixture.Create<Transaction>();
    }
}
