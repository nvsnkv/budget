using NMoneys;

namespace NVs.Budget.Domain.ValueObjects;

public class ExchangeRate
{
    public ExchangeRate(DateTime asOf, Currency from, Currency to, decimal rate)
    {
        if (from == to) throw new ArgumentException("Currencies must be different!", nameof(to));
        if (rate <= 0) throw new ArgumentException("Rate must be a positive value!");

        AsOf = asOf;
        From = from;
        To = to;
        Rate = rate;
    }

    public DateTime AsOf { get; }

    public Currency From { get; }

    public Currency To { get; }

    public decimal Rate { get; }
}
