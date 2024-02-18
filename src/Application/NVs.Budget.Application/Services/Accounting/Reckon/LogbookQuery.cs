using System.Linq.Expressions;
using NMoneys;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Application.Services.Accounting.Reckon;

public record LogbookQuery(
    Criterion LogbookCriterion,
    Expression<Func<TrackedTransaction, bool>>? Conditions = null,
    Currency? OutputCurrency = null,
    bool ExcludeTransfers = false
    ) : TransactionQuery(Conditions, OutputCurrency, ExcludeTransfers);
