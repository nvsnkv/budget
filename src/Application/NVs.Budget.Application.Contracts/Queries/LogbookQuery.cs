using System.Linq.Expressions;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.Contracts.Queries;

public record LogbookQuery(
    Criterion LogbookCriterion,
    Currency OutputCurrency,
    Expression<Func<TrackedOperation, bool>>? Conditions = null,
    bool ExcludeTransfers = true
    ) : OperationQuery(Conditions, OutputCurrency, ExcludeTransfers);
