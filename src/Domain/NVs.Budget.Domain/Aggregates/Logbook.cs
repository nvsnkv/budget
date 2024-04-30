using FluentResults;
using NMoneys;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Errors;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Domain.Aggregates;

public class Logbook
{
    private Currency _currency = Currency.Xxx;
    private readonly List<DateTime> _timestamps = new();
    private readonly List<Operation> _operations = new();

    public virtual Money Sum => _operations.Any() ? _operations.Select(t => t.Amount).Aggregate((left, right) => left + right) : Money.Zero();

    public DateTime From => _operations.FirstOrDefault()?.Timestamp ?? DateTime.MinValue;

    public DateTime Till => _operations.LastOrDefault()?.Timestamp ?? DateTime.MaxValue;

    public bool IsEmpty => !_operations.Any();

    public IEnumerable<Operation> Operations => _operations.AsReadOnly();

    public virtual Logbook this[DateTime from, DateTime till] {
        get
        {
            if (IsEmpty) { return new Logbook(); }

            if (from < From) { from = From; }
            if (till > Till) { till = Till; }

            var logbook = CreateSubRangedLogbook();
            var i = 0;
            while (i<_operations.Count && _operations[i].Timestamp < from) { i++; }

            while (i < _operations.Count && _operations[i].Timestamp <= till)
            {
                var result = logbook.Register(_operations[i++]);
                if (result.IsFailed)
                {
                    throw new InvalidOperationException("Failed to register transaction in child logbook!").WithData("errors", result.Errors);
                }
            }

            return logbook;
        }
    }

    public virtual Result Register(Operation o)
    {
        var currency = o.Amount.GetCurrency();

        if (!_operations.Any())
        {
            _currency = currency;
            _timestamps.Add(o.Timestamp);
            _operations.Add(o);
            return Result.Ok();
        }

        if (currency != _currency)
        {
            return Result.Fail(new UnexpectedCurrencyError(_currency, o));
        }

        var idx = _timestamps.BinarySearch(o.Timestamp);
        if (idx < 0) { idx = ~idx; }

        _timestamps.Insert(idx, o.Timestamp);
        _operations.Insert(idx, o);

        return Result.Ok();
    }

    protected virtual Logbook CreateSubRangedLogbook() => new();
}
