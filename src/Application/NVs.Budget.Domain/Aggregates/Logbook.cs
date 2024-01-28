using FluentResults;
using NMoneys;
using NVs.Budget.Domain.Entities.Transactions;
using NVs.Budget.Domain.Errors;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Domain.Aggregates;

public class Logbook
{
    private Currency _currency = Currency.Xxx;
    private readonly List<DateTime> _timestamps = new();
    private readonly List<Transaction> _transactions = new();

    public virtual Money Sum => _transactions.Any() ? _transactions.Select(t => t.Amount).Aggregate((left, right) => left + right) : Money.Zero();

    public DateTime From => _transactions.FirstOrDefault()?.Timestamp ?? DateTime.MinValue;

    public DateTime Till => _transactions.LastOrDefault()?.Timestamp ?? DateTime.MaxValue;

    public bool IsEmpty => !_transactions.Any();

    public IEnumerable<Transaction> Transactions => _transactions.AsReadOnly();

    public virtual Logbook this[DateTime from, DateTime till] {
        get
        {
            if (IsEmpty) { return new Logbook(); }

            if (from < From) { from = From; }
            if (till > Till) { till = Till; }

            var logbook = CreateSubRangedLogbook();
            var i = 0;
            while (i<_transactions.Count && _transactions[i].Timestamp < from) { i++; }

            while (i < _transactions.Count && _transactions[i].Timestamp <= till)
            {
                var result = logbook.Register(_transactions[i++]);
                if (result.IsFailed)
                {
                    throw new InvalidOperationException("Failed to register transaction in child logbook!").WithData("errors", result.Errors);
                }
            }

            return logbook;
        }
    }

    public virtual Result Register(Transaction t)
    {
        var currency = t.Amount.GetCurrency();

        if (!_transactions.Any())
        {
            _currency = currency;
            _timestamps.Add(t.Timestamp);
            _transactions.Add(t);
            return Result.Ok();
        }

        if (currency != _currency)
        {
            return Result.Fail(new UnexpectedCurrencyError());
        }

        var idx = _timestamps.BinarySearch(t.Timestamp);
        if (idx < 0) { idx = ~idx; }

        _timestamps.Insert(idx, t.Timestamp);
        _transactions.Insert(idx, t);

        return Result.Ok();
    }

    protected virtual Logbook CreateSubRangedLogbook() => new();
}
