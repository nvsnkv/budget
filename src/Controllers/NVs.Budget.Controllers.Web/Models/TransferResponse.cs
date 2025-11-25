using FluentResults;

namespace NVs.Budget.Controllers.Web.Models;

public record TransferResponse(
    Guid SourceId,
    OperationResponse Source,
    Guid SinkId,
    OperationResponse Sink,
    MoneyResponse Fee,
    string Comment,
    string Accuracy
);

public record TransfersListResponse(
    IReadOnlyCollection<TransferResponse> Recorded,
    IReadOnlyCollection<TransferResponse> Unregistered
);

public record RegisterTransferRequest(
    Guid SourceId,
    Guid SinkId,
    MoneyResponse? Fee,
    string Comment,
    string Accuracy
);

public record RegisterTransfersRequest(
    IReadOnlyCollection<RegisterTransferRequest> Transfers
);

public record RemoveTransfersRequest(
    IReadOnlyCollection<Guid> SourceIds,
    bool All
);

