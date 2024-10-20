namespace NVs.Budget.Infrastructure.IO.Console.Output;

public interface IObjectWriter<in T>
{
    Task Write(T obj, CancellationToken ct);

    Task Write(IEnumerable<T> collection, CancellationToken ct);
}
