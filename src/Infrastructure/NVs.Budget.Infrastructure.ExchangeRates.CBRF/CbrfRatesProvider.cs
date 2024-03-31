using System.Globalization;
using System.Xml;
using NMoneys;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.ExchangeRates.Contracts;

namespace NVs.Budget.Infrastructure.ExchangeRates.CBRF;

internal class CbrfRatesProvider : IExchangeRatesProvider
{
    private static readonly Currency HomelandCurrency = Currency.Rub;
    private static readonly string RequestUrl = "http://www.cbr.ru/scripts/XML_daily.asp?date_req=";

    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("ru-RU");

    public async Task<ExchangeRate> Get(DateTime asOf, Currency from, Currency to, CancellationToken ct)
    {
        if (from != HomelandCurrency && to != HomelandCurrency)
        {
            throw new InvalidOperationException("Unable to get rate for conversion that does not involve rubles!");
        }

        var foreignCurrency = from == HomelandCurrency ? to : from;

        var rate = await GetRate(foreignCurrency.IsoSymbol, asOf, ct);
        if (!rate.HasValue)
        {
            throw new InvalidOperationException("Failed to get rate! Script did not return rate for this currency!");
        }

        if (to != HomelandCurrency)
        {
            rate = 1 / rate;
        }

        return new ExchangeRate(asOf, from, to, rate.Value);
    }

    private static async Task<decimal?> GetRate(string foreignCurrencyCode, DateTime requestDate, CancellationToken ct)
    {
        var request = new Uri(RequestUrl + requestDate.ToString("dd/MM/yyyy"));
        using var client = new HttpClient();
        var stream = await client.GetStreamAsync(request, ct);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
        while (await reader.ReadAsync())
        {
            if (reader.Name == "Valute")
            {
                var currency = string.Empty;
                decimal? rate = null;
                while (await reader.ReadAsync())
                {
                    switch (reader.Name)
                    {
                        case "CharCode":
                            currency = await reader.ReadElementContentAsStringAsync();
                            break;


                        case "Value":
                            var value = await reader.ReadElementContentAsStringAsync();
                            rate = decimal.TryParse(value, NumberStyles.Any, Culture, out var d) ? d : null;
                            break;

                    }

                    if (currency == foreignCurrencyCode && rate.HasValue)
                    {
                        return rate;
                    }

                    if (!string.IsNullOrEmpty(currency) && rate.HasValue)
                    {
                        break;
                    }

                }
            }
        }

        return null;
    }
}
