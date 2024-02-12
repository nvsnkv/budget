using NMoneys;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public record UnregisteredTransaction(
    DateTime Timestamp,
    Money Amount,
    string Description,
    IReadOnlyDictionary<string, object>? Attributes,
    UnregisteredAccount Account
);
