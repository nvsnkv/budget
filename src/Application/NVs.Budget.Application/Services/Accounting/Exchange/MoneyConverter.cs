using NMoneys;
using NVs.Budget.Application.Entities.Contracts;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Application.Services.Accounting.Exchange;

internal class MoneyConverter(IExchangeRatesRepository repository, IExchangeRatesProvider provider, IUser currentUser)
{
    public async Task<Operation> Convert(Operation operation, Currency targetCurrency, CancellationToken ct)
    {
        var sourceCurrency = operation.Amount.GetCurrency();
        if (sourceCurrency == targetCurrency)
        {
            return operation;
        }

        var rate = await repository.GetRate(currentUser.AsOwner(), operation.Timestamp, sourceCurrency, targetCurrency, ct);
        if (rate is null)
        {
            rate = await provider.Get(operation.Timestamp, sourceCurrency, targetCurrency, ct);
            await repository.Add(rate, currentUser.AsOwner(), ct);
        }

        return new Operation(
            operation.Id,
            operation.Timestamp,
            new Money(operation.Amount.Amount * rate.Rate, targetCurrency),
            operation.Description,
            operation.Account,
            operation.Tags.ToList(),
            new Dictionary<string, object>(operation.Attributes)
        );
    }
}
