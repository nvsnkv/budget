using FluentResults;
using MediatR;

namespace NVs.Budget.Application.Contracts.UseCases.Transfers;

public record RemoveTransfersCommand(Guid[] SourceIds) : IRequest<Result>;
