﻿using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Services.Accounting;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Exchange;
using NVs.Budget.Application.Services.Accounting.Reckon;
using NVs.Budget.Application.Services.Accounting.Results;
using NVs.Budget.Application.Services.Accounting.Tags;
using NVs.Budget.Application.Services.Accounting.Transfers;
using NVs.Budget.Infrastructure.ExchangeRates.Contracts;
using NVs.Budget.Infrastructure.Identity.Contracts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application;

public sealed class AppServicesFactory(
    IAccountsRepository accountsRepository,
    IOperationsRepository operationsRepository,
    ITransfersRepository transfersRepository,
    IExchangeRatesRepository ratesRepository,
    IExchangeRatesProvider ratesProvider,
    UserCache userCache,
    IReadOnlyCollection<TaggingCriterion> taggingCriteria,
    IReadOnlyList<TransferCriterion> transferCriteria)
{
    public DuplicatesDetectorOptions DuplicatesDetectorOptions { get; set; } = DuplicatesDetectorOptions.Default;

    public IAccountManager CreateAccountManager() => new AccountManager(accountsRepository, userCache.CachedUser);
    public IReckoner CreateReckoner() => new Reckoner(operationsRepository, transfersRepository, CreateMoneyConverter(), CreateDuplicatesDetector(), CreateAccountManager());
    public IAccountant CreateAccountant() => new Accountant(operationsRepository, transfersRepository, CreateAccountManager(), CreateTagsManager(), CreateTransferListBuilder(), new ImportResultBuilder(CreateDuplicatesDetector()));

    private MoneyConverter CreateMoneyConverter() => new(ratesRepository, ratesProvider, userCache.CachedUser);
    private DuplicatesDetector CreateDuplicatesDetector() => new(DuplicatesDetectorOptions);
    private TagsManager CreateTagsManager() => new TagsManager(taggingCriteria);
    private TransfersListBuilder CreateTransferListBuilder() => new(CreateTransfersDetector());
    private TransferDetector CreateTransfersDetector() => new(transferCriteria);
}

public sealed class UserCache(IIdentityService service)
{
    private IUser? _user;
    public IUser CachedUser => _user ?? throw new InvalidOperationException("Cache is not initialized yet!");

    public async Task EnsureInitialized(CancellationToken ct)
    {
        if (_user is not null) return;

        _user = await service.GetCurrentUser(ct);
    }
}
