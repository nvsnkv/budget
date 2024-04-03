using FluentResults;
using MediatR;
using MediatR.Pipeline;
using NVs.Budget.Controllers.Console.IO;

namespace NVs.Budget.Controllers.Console.Behaviors;

internal class ResultWritingBehaviour<TRequest, TResponse>(ResultWriter<TResponse> writer) : IRequestPostProcessor<TRequest, TResponse> where TRequest : IRequest<TResponse> where TResponse : IResultBase
{
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken) => writer.Write(response, cancellationToken);
}
