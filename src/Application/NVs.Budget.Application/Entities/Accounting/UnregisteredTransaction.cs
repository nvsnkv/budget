using NMoneys;

namespace NVs.Budget.Application.Entities.Accounting;

public record UnregisteredTransaction(
    DateTime Timestamp,
    Money Amount,
    string Description,
    IReadOnlyDictionary<string, object>? Attributes,
    UnregisteredAccount Account
);
