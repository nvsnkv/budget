using FluentResults;

namespace NVs.Budget.Infrastructure.IO.Console.Output;

public interface IResultWriter<in T> where T : IResultBase
{
    Task Write(T result, CancellationToken ct);
}
