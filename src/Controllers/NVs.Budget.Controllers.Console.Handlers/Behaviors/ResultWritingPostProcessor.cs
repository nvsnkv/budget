using FluentResults;
using MediatR;
using MediatR.Pipeline;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Behaviors;

internal class ResultWritingPostProcessor<TRequest, TResponse>(IResultWriter<TResponse> writer) : IRequestPostProcessor<TRequest, TResponse>
where TRequest : IRequest<TResponse>
where TResponse : IResultBase
{
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken) => writer.Write(response, cancellationToken);
}
