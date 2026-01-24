using NMoneys;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredMoney(decimal amount, CurrencyIsoCode currencyCode)
{
    public decimal Amount { get; private set; } = amount;
    public CurrencyIsoCode CurrencyCode { get; private set; } = currencyCode;
    public static readonly StoredMoney Zero = new(0, CurrencyIsoCode.XXX);
}
