using System.ComponentModel.Design;
using NVs.Budget.Infrastructure.ExchangeRates.Contracts;

namespace NVs.Budget.Infrastructure.ExchangeRates.CBRF;

public sealed class Factory
{
    public IExchangeRatesProvider CreateProvider() => new CbrfRatesProvider();
}
