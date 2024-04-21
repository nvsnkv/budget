namespace NVs.Budget.Controllers.Console.Handlers;

public interface IEntryPoint
{
    Task<int> Process(IEnumerable<string> args, CancellationToken ct);
}
