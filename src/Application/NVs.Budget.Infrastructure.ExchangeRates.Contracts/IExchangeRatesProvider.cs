using NMoneys;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Infrastructure.ExchangeRates.Contracts;

public interface IExchangeRatesProvider
{
    Task<ExchangeRate> Get(DateTime asOf, Currency from, Currency to, CancellationToken ct);
}
