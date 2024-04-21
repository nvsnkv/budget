using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Results;

namespace NVs.Budget.Application.Contracts.UseCases.Operations;

public record ImportOperationsCommand(
    IAsyncEnumerable<UnregisteredOperation> Operations,
    ImportOptions Options
) : IRequest<ImportResult>;
