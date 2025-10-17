using FluentResults;

namespace NVs.Budget.Controllers.Web.Models;

public record OperationResponse(
    Guid Id,
    string Version,
    DateTime Timestamp,
    MoneyResponse Amount,
    string Description,
    Guid BudgetId,
    IReadOnlyCollection<string> Tags,
    Dictionary<string, object>? Attributes
);

public record MoneyResponse(
    decimal Value,
    string CurrencyCode
);

public record UnregisteredOperationRequest(
    DateTime Timestamp,
    MoneyResponse Amount,
    string Description,
    Dictionary<string, object>? Attributes
);

public record UpdateOperationRequest(
    Guid Id,
    string Version,
    DateTime Timestamp,
    MoneyResponse Amount,
    string Description,
    IReadOnlyCollection<string> Tags,
    Dictionary<string, object>? Attributes
);

public record UpdateOperationsRequest(
    string BudgetVersion,
    IReadOnlyCollection<UpdateOperationRequest> Operations,
    string? TransferConfidenceLevel,
    string TaggingMode
);

public record RemoveOperationsRequest(
    string Criteria
);

// Result response models
public record ImportResultResponse(
    IReadOnlyCollection<OperationResponse> RegisteredOperations,
    IReadOnlyCollection<IReadOnlyCollection<OperationResponse>> Duplicates,
    IReadOnlyCollection<IError> Errors,
    IReadOnlyCollection<ISuccess> Successes);

public record UpdateResultResponse(
    IReadOnlyCollection<OperationResponse> UpdatedOperations,
    IReadOnlyCollection<IError> Errors,
    IReadOnlyCollection<ISuccess> Successes
);

