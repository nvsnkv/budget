using System.Linq.Expressions;
using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal sealed class PersistenceMapper(ReadableExpressionsParser parser)
{
    public static readonly IReadOnlyDictionary<Type, Type> TypeMappings = new Dictionary<Type, Type>
    {
        { typeof(Money), typeof(StoredMoney) },
        { typeof(Tag), typeof(StoredTag) },
        { typeof(Owner), typeof(StoredOwner) },
        { typeof(TrackedOwner), typeof(StoredOwner) },
        { typeof(TrackedBudget), typeof(StoredBudget) },
        { typeof(TrackedOperation), typeof(StoredOperation) },
        { typeof(ExchangeRate), typeof(StoredRate) },
        { typeof(TrackedTransfer), typeof(StoredTransfer) }
    };

    public StoredMoney ToStored(Money money) => new(money.Amount, money.CurrencyCode);

    public Money ToMoney(StoredMoney stored) => new(stored.Amount, Currency.Get(stored.CurrencyCode));

    public StoredTag ToStored(Tag tag) => new(tag.Value);

    public Tag ToTag(StoredTag stored) => new(stored.Value);

    public StoredOwner ToStored(Owner owner) => new(owner.Id, owner.Name);

    public Owner ToOwner(StoredOwner stored) => new(stored.Id, stored.Name);

    public StoredOwner ToStored(TrackedOwner owner)
    {
        var stored = new StoredOwner(owner.Id, owner.Name) { Version = owner.Version };
        return stored;
    }

    public TrackedOwner ToTrackedOwner(StoredOwner stored) => new(stored.Id, stored.Name) { Version = stored.Version };

    public StoredBudget ToStored(Domain.Entities.Budgets.Budget budget)
    {
        var stored = new StoredBudget(budget.Id, budget.Name);
        foreach (var owner in budget.Owners)
        {
            stored.Owners.Add(ToStored(owner));
        }

        return stored;
    }

    public Domain.Entities.Budgets.Budget ToBudget(StoredBudget stored) => new(stored.Id, stored.Name, stored.Owners.Select(ToOwner));

    public StoredBudget ToStored(TrackedBudget budget)
    {
        var stored = new StoredBudget(budget.Id, budget.Name) { Version = budget.Version };
        foreach (var owner in budget.Owners)
        {
            stored.Owners.Add(ToStored(owner));
        }

        foreach (var criterion in budget.TaggingCriteria)
        {
            stored.TaggingCriteria.Add(ToStored(criterion));
        }

        foreach (var criterion in budget.TransferCriteria)
        {
            stored.TransferCriteria.Add(ToStored(criterion));
        }

        stored.LogbookCriteria.Clear();
        foreach (var criterion in budget.LogbookCriteria)
        {
            stored.LogbookCriteria.Add(ToStored(criterion));
        }

        return stored;
    }

    public TrackedBudget ToTrackedBudget(StoredBudget stored) => new(
        stored.Id,
        stored.Name,
        stored.Owners.Select(ToOwner),
        stored.TaggingCriteria.Select(ToTaggingCriterion),
        stored.TransferCriteria.Select(ToTransferCriterion),
        stored.LogbookCriteria.Select(ToLogbookCriteria)
    )
    {
        Version = stored.Version
    };

    public StoredOperation ToStored(Operation operation)
    {
        var stored = new StoredOperation(operation.Id, operation.Timestamp, operation.Description, operation.Notes)
        {
            Amount = ToStored(operation.Amount),
            Budget = ToStored(operation.Budget),
            Tags = operation.Tags.Select(ToStored).ToList(),
            Attributes = new Dictionary<string, object>(operation.Attributes)
        };

        return stored;
    }

    public StoredOperation ToStored(TrackedOperation operation)
    {
        var stored = new StoredOperation(
            operation.Id,
            operation.Timestamp.ToUniversalTime(),
            operation.Description,
            operation.Notes)
        {
            Amount = ToStored(operation.Amount),
            Budget = ToStored(operation.Budget),
            Tags = operation.Tags.Select(ToStored).ToList(),
            Attributes = new Dictionary<string, object>(operation.Attributes),
            Version = operation.Version
        };

        return stored;
    }

    public TrackedOperation ToTrackedOperation(StoredOperation stored) => new(
        stored.Id,
        SpecifyUtc(stored.Timestamp),
        ToMoney(stored.Amount),
        stored.Description,
        stored.Notes,
        ToBudget(stored.Budget),
        stored.Tags.Select(ToTag),
        stored.Attributes
    )
    {
        Version = stored.Version
    };

    public Operation ToOperation(StoredOperation stored) => new(
        stored.Id,
        SpecifyUtc(stored.Timestamp),
        ToMoney(stored.Amount),
        stored.Description,
        stored.Notes,
        ToBudget(stored.Budget),
        stored.Tags.Select(ToTag),
        stored.Attributes
    );

    public StoredTaggingCriterion ToStored(TaggingCriterion criterion) => new()
    {
        Tag = criterion.Tag.ToString(),
        Condition = criterion.Condition.ToString()
    };

    public TaggingCriterion ToTaggingCriterion(StoredTaggingCriterion stored) => new(
        parser.ParseUnaryConversion<TrackedOperation>(stored.Tag).Value,
        parser.ParseUnaryPredicate<TrackedOperation>(stored.Condition).Value
    );

    public StoredTransferCriterion ToStored(TransferCriterion criterion) => new()
    {
        Accuracy = criterion.Accuracy,
        Comment = criterion.Comment,
        Criterion = criterion.Criterion.ToString()
    };

    public TransferCriterion ToTransferCriterion(StoredTransferCriterion stored) => new(
        stored.Accuracy,
        stored.Comment,
        parser.ParseBinaryPredicate<TrackedOperation, TrackedOperation>(stored.Criterion).Value
    );

    public StoredLogbookCriteria ToStored(LogbookCriteria criteria) => new()
    {
        Description = criteria.Description,
        Subcriteria = criteria.Subcriteria?.Select(ToStored).ToList(),
        Type = criteria.Type,
        Tags = criteria.Tags?.Select(ToStored).ToList(),
        Substitution = criteria.Substitution?.ToString(),
        Criteria = criteria.Criteria?.ToString(),
        IsUniversal = criteria.IsUniversal,
        Precondition = criteria.Precondition?.ToString()
    };

    public LogbookCriteria ToLogbookCriteria(StoredLogbookCriteria stored) => new(
        stored.Description,
        stored.Subcriteria?.Select(ToLogbookCriteria).ToList(),
        stored.Type,
        stored.Tags?.Select(ToTag).ToList(),
        stored.Substitution is null ? null : parser.ParseUnaryConversion<Operation>(stored.Substitution).Value,
        stored.Criteria is null ? null : parser.ParseUnaryPredicate<Operation>(stored.Criteria).Value,
        stored.IsUniversal,
        stored.Precondition is null ? null : parser.ParseUnaryPredicate<Operation>(stored.Precondition).Value
    );

    public StoredTransfer ToStored(TrackedTransfer transfer)
    {
        var stored = new StoredTransfer(transfer.Comment)
        {
            Fee = ToStored(transfer.Fee),
            Source = ToStored(transfer.Source),
            Sink = ToStored(transfer.Sink),
            StartedAt = transfer.StartedAt.ToUniversalTime(),
            CompletedAt = transfer.CompletedAt.ToUniversalTime()
        };

        return stored;
    }

    public TrackedTransfer ToTrackedTransfer(StoredTransfer stored)
    {
        var source = ToOperation(stored.Source);
        var sink = ToOperation(stored.Sink);
        var fee = ToMoney(stored.Fee);

        return fee.Amount == 0 && source.Amount.HasSameCurrencyAs(sink.Amount)
            ? new TrackedTransfer(source, sink, stored.Comment)
            : new TrackedTransfer(source, sink, fee, stored.Comment);
    }

    public StoredRate ToStored(ExchangeRate rate) => new(
        rate.AsOf,
        rate.From.IsoCode,
        rate.To.IsoCode,
        rate.Rate);

    public ExchangeRate? ToExchangeRate(StoredRate? stored) => stored is null
        ? null
        : new ExchangeRate(
            stored.AsOf,
            Currency.Get(stored.From),
            Currency.Get(stored.To),
            stored.Rate);

    private static DateTime SpecifyUtc(DateTime timestamp) => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
}
