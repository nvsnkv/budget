using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Tags;

public record TaggingCriterion(Tag Tag, Func<TrackedTransaction, bool> Criterion);