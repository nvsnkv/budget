using System.Linq.Expressions;
using NMoneys;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Reckon;

public record TransactionQuery(
    Expression<Func<TrackedTransaction, bool>>? Conditions = null,
    Currency? OutputCurrency = null,
    bool ExcludeTransfers = false);