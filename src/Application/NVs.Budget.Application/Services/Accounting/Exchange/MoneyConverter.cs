using NMoneys;
using NVs.Budget.Application.Entities.Contracts;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.Services.Accounting.Exchange;

internal class MoneyConverter(IExchangeRatesRepository repository, IExchangeRatesProvider provider, IUser currentUser)
{
    public async Task<Transaction> Convert(Transaction transaction, Currency targetCurrency, CancellationToken ct)
    {
        var sourceCurrency = transaction.Amount.GetCurrency();
        if (sourceCurrency == targetCurrency)
        {
            return transaction;
        }

        var rate = await repository.GetRate(currentUser.AsOwner(), transaction.Timestamp, sourceCurrency, targetCurrency, ct);
        if (rate is null)
        {
            rate = await provider.Get(transaction.Timestamp, sourceCurrency, targetCurrency, ct);
            await repository.Add(rate, currentUser.AsOwner(), ct);
        }

        return new Transaction(
            transaction.Id,
            transaction.Timestamp,
            new Money(transaction.Amount.Amount * rate.Rate, targetCurrency),
            transaction.Description,
            transaction.Account,
            transaction.Tags.ToList(),
            new Dictionary<string, object>(transaction.Attributes)
        );
    }
}
