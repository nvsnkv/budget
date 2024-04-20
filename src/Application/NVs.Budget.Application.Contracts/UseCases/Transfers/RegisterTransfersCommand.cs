using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.UseCases.Transfers;

public record RegisterTransfersCommand(IAsyncEnumerable<UnregisteredTransfer> Transfers) : IRequest<Result>;
