using NMoneys;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredMoney(decimal amount, CurrencyIsoCode currency)
{
    public decimal Amount { get; private set; } = amount;
    public CurrencyIsoCode Currency { get; private set; } = currency;
    public static readonly StoredMoney Zero = new(0, CurrencyIsoCode.XXX);
}
