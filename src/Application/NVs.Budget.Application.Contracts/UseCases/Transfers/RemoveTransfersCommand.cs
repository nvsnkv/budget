using FluentResults;
using MediatR;

namespace NVs.Budget.Application.Contracts.UseCases.Transfers;

public record RemoveTransfersCommand(Guid[] SourceIds, bool All) : IRequest<Result>;
