using FluentResults;
using MediatR;

namespace NVs.Budget.Application.Contracts.UseCases.Accounts;

public record MergeAccountsCommand(Guid SourceId, Guid TargetId) : IRequest<Result>;
