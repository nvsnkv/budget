using NMoneys;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Domain.Entities.Transactions;

public class Transfer
{
    public static readonly Tag TransferTag = new Tag(nameof(Transfer));

    public Transfer(Transaction source, Transaction sink, string comment)
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
        Fee = sink.Amount - source.Amount;
        Comment = comment;
    }

    public Transfer(Transaction source, Transaction sink, Money fee, string comment)
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

        Source = source;
        Sink = sink;
        Fee = fee;
        Comment = comment;
    }

    public Transaction Source { get; }

    public Transaction Sink { get; }

    public string Comment { get; }

    public Money Fee { get; } = Money.Zero();

    public Transaction AsTransaction()
    {
        var timestamp = Source.Timestamp;
        var amount = Fee;
        var description = Comment;
        var account = Source.Account;

        if (Fee.Amount > 0)
        {
            timestamp = Sink.Timestamp;
            account = Sink.Account;
        }

        var tags = Enumerable.Repeat(TransferTag, 1);
        var attributes = new Dictionary<string, object>()
        {
            { nameof(Source), Source.Id },
            { nameof(Sink), Sink.Id }
        };

        return new Transaction(Guid.Empty, timestamp, amount, description, account, tags, attributes);
    }
}
