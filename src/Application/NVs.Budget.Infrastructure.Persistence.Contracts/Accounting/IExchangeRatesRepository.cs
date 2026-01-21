using NMoneys;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

public interface IExchangeRatesRepository
{
    Task<ExchangeRate?> GetRate(Owner owner, DateTime asOf, Currency from, Currency to, CancellationToken ct);
    Task Add(ExchangeRate rate, Owner owner, CancellationToken ct);
}
