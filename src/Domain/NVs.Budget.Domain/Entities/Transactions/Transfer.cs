using System.Collections;
using NMoneys;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Domain.Entities.Transactions;

public class Transfer : IEnumerable<Operation>
{
    public static readonly Tag TransferTag = new(nameof(Transfer));

    public Transfer(Operation source, Operation sink, string comment)
    {
        if (source.Amount.Amount >= 0)
        {
            throw new ArgumentException("Source must be a withdraw!", nameof(source))
                .WithData(nameof(source), source.Id)
                .WithData(nameof(sink), sink.Id);
        }

        if (sink.Amount.Amount <= 0)
        {
            throw new ArgumentException("Sink must be an income!", nameof(sink))
                .WithData(nameof(source), source.Id)
                .WithData(nameof(sink), sink.Id);
        }

        if (!source.Amount.HasSameCurrencyAs(sink.Amount))
        {
            throw new ArgumentException("Transactions must have same currency when no fee provided!")
                .WithData(nameof(source), source.Id)
                .WithData(nameof(sink), sink.Id);
        }

        Source = source;
        Sink = sink;
        Fee = sink.Amount + source.Amount;
        Comment = comment;
    }

    public Transfer(Operation source, Operation sink, Money fee, string comment)
    {
        if (source.Amount.Amount >= 0)
        {
            throw new ArgumentException("Source must be a withdraw!", nameof(source))
                .WithData(nameof(source), source.Id)
                .WithData(nameof(sink), sink.Id);
        }

        if (sink.Amount.Amount <= 0)
        {
            throw new ArgumentException("Sink must be an income!", nameof(sink))
                .WithData(nameof(source), source.Id)
                .WithData(nameof(sink), sink.Id);
        }

        if (fee.Amount < 0 && !fee.HasSameCurrencyAs(source.Amount))
        {
            throw new ArgumentException("Negative fee should have same currency as source!", nameof(fee))
                .WithData(nameof(source), source.Id)
                .WithData(nameof(sink), sink.Id)
                .WithData(nameof(fee.CurrencyCode), fee.CurrencyCode);
        }

        if (fee.Amount > 0 && !fee.HasSameCurrencyAs(sink.Amount))
        {
            throw new ArgumentException("Positive fee should have same currency as sink!", nameof(fee))
                .WithData(nameof(source), source.Id)
                .WithData(nameof(sink), sink.Id)
                .WithData(nameof(fee.CurrencyCode), fee.CurrencyCode);
        }

        Source = source;
        Sink = sink;
        Fee = fee;
        Comment = comment;
    }

    public Operation Source { get; }

    public Operation Sink { get; }

    public string Comment { get; }

    public Money Fee { get; }

    public Operation AsTransaction()
    {
        var timestamp = Source.Timestamp;
        var amount = Fee;
        var description = Comment;
        var budget = Source.Budget;

        if (Fee.Amount > 0)
        {
            timestamp = Sink.Timestamp;
            budget = Sink.Budget;
        }

        var tags = Enumerable.Repeat(TransferTag, 1);
        var attributes = new Dictionary<string, object>
        {
            { nameof(Source), Source.Id },
            { nameof(Sink), Sink.Id }
        };

        return new Operation(Guid.Empty, timestamp, amount, description, budget, tags, attributes);
    }

    public IEnumerator<Operation> GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class Enumerator(Transfer transfer) : IEnumerator<Operation>
    {
        private Operation? _current;
        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_current is null)
            {
                _current = transfer.Source;
                return true;
            }
            if (_current == transfer.Source)
            {
                _current = transfer.Sink;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _current = null;
        }

        public Operation Current => _current ?? throw new InvalidOperationException("Current should not be used in current state");

        object IEnumerator.Current => Current;
    }
}
