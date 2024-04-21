using FluentResults;

namespace NVs.Budget.Controllers.Console.Contracts.IO.Output;

public interface IResultWriter<in T> where T : IResultBase
{
    Task Write(T result, CancellationToken ct);
}
