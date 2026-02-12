using System.Linq.Expressions;
using FluentResults;
using MediatR;
using NMoneys;
using NVs.Budget.Application.Contracts.UseCases.Budgets;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Controllers.Web.IntegrationTests.Infrastructure;

internal sealed class InMemoryMediator : IMediator
{
    private readonly List<TrackedBudget> _budgets;

    public InMemoryMediator(IEnumerable<TrackedBudget> budgets)
    {
        _budgets = budgets.ToList();
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        object response = request switch
        {
            ListOwnedBudgetsQuery => _budgets.AsReadOnly(),
            UpdateBudgetCommand update => HandleUpdate(update),
            CalcOperationsStatisticsQuery calc => HandleCalc(calc),
            _ => throw new NotSupportedException($"Unsupported request: {request.GetType().Name}")
        };

        return Task.FromResult((TResponse)response);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<TResponse>();
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<object?>();
    }

    private Result HandleUpdate(UpdateBudgetCommand update)
    {
        var current = _budgets.FirstOrDefault(b => b.Id == update.Budget.Id);
        if (current == null)
        {
            return Result.Fail($"Budget {update.Budget.Id} not found");
        }

        var replacement = new TrackedBudget(
            update.Budget.Id,
            update.Budget.Name,
            update.Budget.Owners,
            update.Budget.TaggingCriteria,
            update.Budget.TransferCriteria,
            update.Budget.LogbookCriteria)
        {
            Version = update.Budget.Version
        };
        var idx = _budgets.IndexOf(current);
        _budgets[idx] = replacement;
        return Result.Ok();
    }

    private Result<CriteriaBasedLogbook> HandleCalc(CalcOperationsStatisticsQuery calc)
    {
        var criterion = calc.Criterion;
        var logbook = new CriteriaBasedLogbook(criterion);
        var budget = _budgets.First();
        var operation = new TrackedOperation(
            Guid.NewGuid(),
            DateTime.UtcNow,
            new Money(100, Currency.Get("USD")),
            "Integration operation",
            string.Empty,
            new NVs.Budget.Domain.Entities.Budgets.Budget(budget.Id, budget.Name, budget.Owners),
            [new Tag("integration")],
            new Dictionary<string, object>()
        );
        var registerResult = logbook.Register(operation);
        return registerResult.IsSuccess
            ? Result.Ok(logbook)
            : Result.Fail(registerResult.Errors);
    }
}
