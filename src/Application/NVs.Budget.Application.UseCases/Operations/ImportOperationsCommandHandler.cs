using MediatR;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Operations;

namespace NVs.Budget.Application.UseCases.Operations;

internal class ImportOperationsCommandHandler(IAccountant accountant) : IRequestHandler<ImportOperationsCommand, ImportResult>
{
    public async Task<ImportResult> Handle(ImportOperationsCommand request, CancellationToken cancellationToken)
    {
        return await accountant.ImportOperations(request.Operations, request.Budget, request.Options, cancellationToken);
    }
}
