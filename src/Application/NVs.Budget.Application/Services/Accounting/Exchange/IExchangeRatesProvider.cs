using NMoneys;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Exchange;

public interface IExchangeRatesProvider
{
    Task<ExchangeRate> Get(DateTime asOf, Currency from, Currency to, CancellationToken ct);
}
