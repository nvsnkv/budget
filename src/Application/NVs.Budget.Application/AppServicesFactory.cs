using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Services.Accounting;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Exchange;
using NVs.Budget.Application.Services.Accounting.Reckon;
using NVs.Budget.Application.Services.Accounting.Results;
using NVs.Budget.Infrastructure.ExchangeRates.Contracts;
using NVs.Budget.Infrastructure.Identity.Contracts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application;

public sealed class AppServicesFactory(
    IBudgetsRepository budgetsRepository,
    IOperationsRepository operationsRepository,
    IStreamingOperationRepository streamingOperationRepository,
    ITransfersRepository transfersRepository,
    IExchangeRatesRepository ratesRepository,
    IExchangeRatesProvider ratesProvider,
    UserCache userCache)
{
    public DuplicatesDetectorOptions DuplicatesDetectorOptions { get; set; } = DuplicatesDetectorOptions.Default;

    public IBudgetManager CreateAccountManager() => new BudgetManager(budgetsRepository, userCache.CachedUser);
    public IReckoner CreateReckoner() => new Reckoner(streamingOperationRepository, transfersRepository, CreateMoneyConverter(), CreateDuplicatesDetector(), CreateAccountManager());
    public IAccountant CreateAccountant() => new Accountant(operationsRepository, streamingOperationRepository, transfersRepository, CreateAccountManager(), new ImportResultBuilder(CreateDuplicatesDetector()));

    private MoneyConverter CreateMoneyConverter() => new(ratesRepository, ratesProvider, userCache.CachedUser);
    private DuplicatesDetector CreateDuplicatesDetector() => new(DuplicatesDetectorOptions);
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
