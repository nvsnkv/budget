namespace NVs.Budget.Controllers.Console.Contracts.IO.Output;

public interface IObjectWriter<in T>
{
    Task Write(T obj, CancellationToken ct);
}
