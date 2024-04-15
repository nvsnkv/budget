using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Services.Accounting;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Exchange;
using NVs.Budget.Application.Services.Accounting.Reckon;
using NVs.Budget.Application.Tests.Fakes;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Infrastructure.ExchangeRates.Contracts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Tests;

public class ReckonerShould
{
    private readonly Fixture _fixture =new();
    private readonly Storage _storage = new();
    private readonly Mock<IExchangeRatesProvider> _ratesProvider = new();
    private readonly Reckoner _reckoner;
    private readonly ReckonerTestData _data;
    private readonly Owner _currentOwner;

    public ReckonerShould()
    {
        _currentOwner = _fixture.Create<Owner>();
        Mock<IUser> user = new();
        user.Setup(u => u.AsOwner()).Returns(_currentOwner);


        var converter = new MoneyConverter(new Mock<IExchangeRatesRepository>().Object, _ratesProvider.Object, user.Object);
        var manager = new AccountManager(_storage.Accounts, user.Object);

        _reckoner = new Reckoner(
            _storage.Operations,
            _storage.Transfers,
            converter,
            new DuplicatesDetector(DuplicatesDetectorOptions.Default),
            manager);

        _data = new ReckonerTestData(_currentOwner, 2, 4, 6);
        _storage.Accounts.Append(_data.AllAccounts);
        _storage.Operations.Append(_data.AllTransactions);

    }

    [Fact]
    public async Task ReturnOnlyAccessibleTransactionsThatMatchesQuery()
    {
        var filterIds = _data.AllTransactions.Select((t, i) => i % 2 == 0 ? t : null)
            .Where(t => t is not null)
            .Select(t => t!.Id)
            .ToList();

        var query = new OperationQuery(t => filterIds.Contains(t.Id));
        var expectedTransactions = _data.OwnedTransactions.Where(query.Conditions!.Compile()).ToList();

        var actual = await _reckoner.GetTransactions(query, CancellationToken.None).ToListAsync();

        actual.Should().BeEquivalentTo(expectedTransactions);
        actual.Select(a => a.Account).Should().AllSatisfy(a => a.Owners.Contains(_currentOwner).Should().BeTrue());
    }

    [Fact]
    public async Task CreateLogbookOnlyFromAccessibleTransactionsThatMatchesCriteria()
    {
        var filterIds = _data.AllTransactions.Select((t, i) => i % 2 == 0 ? t : null)
            .Where(t => t is not null)
            .Select(t => t!.Id)
            .ToList();

        var expectedCurrency = _fixture.Create<Currency>();
        var rate = 1.25m;
        _ratesProvider
            .Setup(p => p.Get(It.IsAny<DateTime>(), It.IsAny<Currency>(), expectedCurrency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExchangeRate(DateTime.Today, Currency.Xxx, expectedCurrency, rate));

        var query = new LogbookQuery(LogbookCriterion: new UniversalCriterion("Everyone"),OutputCurrency: expectedCurrency, t => filterIds.Contains(t.Id));
        var expectedTransactions = _data.OwnedTransactions.Where(query.Conditions!.Compile()).ToList();

        var logbook = await _reckoner.GetLogbook(query, CancellationToken.None);
        logbook.Operations.Select(t => t.Id).Should().BeEquivalentTo(expectedTransactions.Select(t => t.Id));
        logbook.Operations.Should().AllSatisfy(t => t.Amount.GetCurrency().Should().Be(expectedCurrency));
    }

    [Fact]
    public async Task ConvertCurrenciesIfRequested()
    {
        var expectedCurrency = _fixture.Create<Currency>();
        var rate = 1.25m;
        _ratesProvider
            .Setup(p => p.Get(It.IsAny<DateTime>(), It.IsAny<Currency>(), expectedCurrency, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExchangeRate(DateTime.Today, Currency.Xxx, expectedCurrency, rate));
        var expectedAmounts = _data.OwnedTransactions
            .OrderBy(t => t.Id)
            .Select(t => t.Amount.GetCurrency() == expectedCurrency ? t.Amount.Amount : t.Amount.Amount * rate)
            .ToList();

        var actual = await _reckoner.GetTransactions(new OperationQuery(OutputCurrency: expectedCurrency), CancellationToken.None).ToListAsync();
        actual.Should().AllSatisfy(t => t.Amount.GetCurrency().Should().Be(expectedCurrency));
        actual.OrderBy(t => t.Id).Select(t => t.Amount.Amount).Should().BeEquivalentTo(expectedAmounts);
    }

    [Fact]
    public async Task ExcludeTransfersIfRequestedAndReplaceThemWithVirtualTransactions()
    {
        var accessibleTransferWithFee = new TrackedTransfer(_data.OwnedTransactions.First(), _data.OwnedTransactions.Last(), new Money(-5, _data.OwnedTransactions.First().Amount.CurrencyCode), _fixture.Create<string>());
        var accessibleTransferWithoutFee = new TrackedTransfer(_data.OwnedTransactions.Skip(1).First(), _data.OwnedTransactions.Reverse().Skip(1).First(), Money.Zero(), _fixture.Create<string>());
        var inaccessibleTransfer = new TrackedTransfer(_data.OwnedTransactions.Skip(2).First(), _data.NotOwnedTransactions.Last(), Money.Zero(), _fixture.Create<string>());

        _storage.Transfers.Append(new[] {accessibleTransferWithFee, inaccessibleTransfer, accessibleTransferWithoutFee});

        var actual = await _reckoner.GetTransactions(new OperationQuery(ExcludeTransfers: true), CancellationToken.None).ToListAsync();

        using var scope = new AssertionScope
        {
            FormattingOptions = { MaxLines = 10000 }
        };

        actual.Should().NotContain(accessibleTransferWithFee.Cast<TrackedOperation>());
        actual.Should().NotContain(accessibleTransferWithoutFee.Cast<TrackedOperation>());
        actual.Should().Contain(inaccessibleTransfer.Where(t => t.Account.Owners.Contains(_currentOwner)).Cast<TrackedOperation>());
        actual.Should().NotContain(inaccessibleTransfer.Where(t => !t.Account.Owners.Contains(_currentOwner)).Cast<TrackedOperation>());
        actual.Any(a =>CheckIfTheSameTransactions(a, accessibleTransferWithFee.AsTransaction())).Should().BeTrue();
        actual.Any(a => CheckIfTheSameTransactions(a, inaccessibleTransfer.AsTransaction())).Should().BeFalse();
        actual.Any(a => CheckIfTheSameTransactions(a, accessibleTransferWithoutFee.AsTransaction())).Should().BeFalse();
    }

    [Fact]
    public async Task DetectDuplicates()
    {
        _storage.Operations.Append(Enumerable.Repeat(_data.OwnedTransactions[0], 3));
        _storage.Operations.Append(Enumerable.Repeat(_data.OwnedTransactions[^1], 2));
        _storage.Operations.Append(Enumerable.Repeat(_data.NotOwnedTransactions[^1], 2));

        var duplicates = await _reckoner.GetDuplicates(_ => true, CancellationToken.None);
        duplicates.Should().HaveCount(2);
        foreach (var duplicate in duplicates)
        {
            if (duplicate.First() == _data.OwnedTransactions[0])
            {
                duplicate.Should().HaveCount(4);
                duplicate.Should().AllBeEquivalentTo(_data.OwnedTransactions[0]);
            }
            else
            {
                duplicate.Should().HaveCount(3);
                duplicate.Should().AllBeEquivalentTo(_data.OwnedTransactions[^1]);
            }
        }

    }

    private bool CheckIfTheSameTransactions(Operation left, Operation right)
    {
        return left.Amount == right.Amount
               && left.Description == right.Description
               && left.Timestamp == right.Timestamp
               && left.Account == right.Account;
    }
}
