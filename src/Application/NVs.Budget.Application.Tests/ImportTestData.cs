using AutoFixture;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

internal class ImportTestData
{
    private readonly UnregisteredOperation[] _justTransactions;
    private readonly UnregisteredOperation[] _transfer;
    private readonly UnregisteredOperation[] _duplicates;

    public ImportTestData(Fixture fixture, IEnumerable<TrackedBudget> knownAccounts)
    {
        UnregisteredBudget[] budgets = fixture.CreateMany<UnregisteredBudget>().Take(2)
            .Concat(knownAccounts.Select(a => new UnregisteredBudget(a.Name)))
            .ToArray();

        _justTransactions = budgets.SelectMany(a =>
        {
            using (fixture.SetCurrency(fixture.Create<CurrencyIsoCode>()))
            {
                return fixture.CreateMany<UnregisteredOperation>(5);
            }
        }).ToArray();

        fixture.SetNamedParameter<decimal>(nameof(Money.Amount), -900);
        var source = fixture.Create<UnregisteredOperation>();
        var sink = source with { Amount = source.Amount * -1, Attributes = new Dictionary<string, object>() };
        _transfer = [source, sink];
        fixture.ResetNamedParameter<decimal>(nameof(Money.Amount));

        _duplicates = fixture.CreateMany<UnregisteredOperation>().Take(2).SelectMany(t => Enumerable.Repeat(t, 2)).ToArray();
    }

    public IAsyncEnumerable<UnregisteredOperation> Operations => _justTransactions.Concat(_transfer).Concat(_duplicates).ToAsyncEnumerable();

    public void VerifyResult(ImportResult result)
    {
        var expectedTransactions = _justTransactions.Concat(_transfer).Concat(_duplicates);

        result.IsSuccess.Should().BeTrue();
        result.Operations.Should().BeEquivalentTo(expectedTransactions);

        result.Transfers.Should().HaveCount(1);
        var transfer = result.Transfers.Single();

        var (expectedSource, expectedSink) = _transfer[0].Amount.IsNegative() ? (_transfer[0], _transfer[1]) : (_transfer[1], _transfer[0]);
        transfer.Source.Should().BeEquivalentTo(expectedSource);
        transfer.Sink.Should().BeEquivalentTo(expectedSink);

        result.Duplicates.Should().HaveCount(2);
    }

}
