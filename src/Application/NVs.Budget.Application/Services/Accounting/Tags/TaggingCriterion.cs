using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Tags;

internal record TaggingCriterion(Func<TrackedOperation, Tag> Tag, Func<TrackedOperation, bool> Criterion);
