using NMoneys;

namespace NVs.Budget.Application.Contracts.Entities.Budgeting;

public record UnregisteredOperation(
    DateTime Timestamp,
    Money Amount,
    string Description,
    IReadOnlyDictionary<string, object>? Attributes
);
