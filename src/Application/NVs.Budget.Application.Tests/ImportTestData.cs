using AutoFixture;
using FluentAssertions;
using NMoneys;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Results;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

internal class ImportTestData
{
    private readonly Owner _owner;
    private readonly UnregisteredAccount[] _accounts;
    private readonly UnregisteredOperation[] _justTransactions;
    private readonly UnregisteredOperation[] _transfer;
    private readonly UnregisteredOperation[] _duplicates;

    public ImportTestData(Fixture fixture, IEnumerable<TrackedAccount> knownAccounts, Owner owner)
    {
        _owner = owner;
        _accounts = fixture.CreateMany<UnregisteredAccount>().Take(2)
            .Concat(knownAccounts.Select(a => new UnregisteredAccount(a.Name, a.Bank)))
            .ToArray();

        _justTransactions = _accounts.SelectMany(a =>
        {
            using (fixture.SetNamedParameter(nameof(UnregisteredOperation.Account), a))
            using (fixture.SetCurrency(fixture.Create<CurrencyIsoCode>()))
            {
                return fixture.CreateMany<UnregisteredOperation>(5);
            }
        }).ToArray();

        fixture.SetNamedParameter(nameof(UnregisteredOperation.Account), _accounts[0]);
        fixture.SetNamedParameter<decimal>(nameof(Money.Amount), -900);
        var source = fixture.Create<UnregisteredOperation>();
        var sink = new UnregisteredOperation(source.Timestamp, source.Amount * -1, source.Description, new Dictionary<string, object>(), _accounts[1]);
        _transfer = [source, sink];
        fixture.ResetNamedParameter<UnregisteredAccount>(nameof(UnregisteredOperation.Account));
        fixture.ResetNamedParameter<decimal>(nameof(Money.Amount));

        fixture.SetNamedParameter(nameof(UnregisteredOperation.Account), _accounts[^1]);
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
