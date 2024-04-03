namespace NVs.Budget.Controllers.Console;

public interface IEntryPoint
{
    Task<int> Process(IEnumerable<string> args, CancellationToken ct);
}
