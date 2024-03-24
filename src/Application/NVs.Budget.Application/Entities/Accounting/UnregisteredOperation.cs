using NMoneys;

namespace NVs.Budget.Application.Entities.Accounting;

public record UnregisteredOperation(
    DateTime Timestamp,
    Money Amount,
    string Description,
    IReadOnlyDictionary<string, object>? Attributes,
    UnregisteredAccount Account
);
