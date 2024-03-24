using NMoneys;

namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class StoredRate(DateTime asOf, CurrencyIsoCode from, CurrencyIsoCode to, decimal rate)
{
    public DateTime AsOf { get; private set; } = asOf;
    public  CurrencyIsoCode From { get; private set; } = from;
    public  CurrencyIsoCode To { get; private set; } = to;
    public  decimal Rate { get; private set; } = rate;

    public virtual StoredOwner Owner { get; init; } = StoredOwner.Invalid;
}
