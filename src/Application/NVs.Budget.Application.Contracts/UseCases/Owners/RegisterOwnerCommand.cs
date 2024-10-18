using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Contracts.UseCases.Owners;

public record RegisterOwnerCommand(IUser User) : IRequest<Result<TrackedOwner>>;
