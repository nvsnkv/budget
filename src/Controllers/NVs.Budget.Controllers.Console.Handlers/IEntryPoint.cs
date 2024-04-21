namespace NVs.Budget.Controllers.Console.Handlers;

public interface IEntryPoint
{
    Task<int> Process(string[] args, CancellationToken ct);
}
