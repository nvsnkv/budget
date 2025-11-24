using System.Linq.Expressions;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.Queries;

public record OperationQuery(
    Expression<Func<TrackedOperation, bool>>? Conditions = null,
    Currency? OutputCurrency = null,
    bool ExcludeTransfers = false);
