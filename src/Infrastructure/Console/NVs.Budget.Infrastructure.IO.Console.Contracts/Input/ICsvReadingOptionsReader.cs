using FluentResults;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface ICsvReadingOptionsReader
{
    Task<Result<CsvReadingOptions>> ReadFrom(StreamReader reader, CancellationToken ct);
}
