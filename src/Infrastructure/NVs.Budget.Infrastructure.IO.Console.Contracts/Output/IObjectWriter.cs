namespace NVs.Budget.Infrastructure.IO.Console.Output;

public interface IObjectWriter<in T>
{
    Task Write(T criterion, CancellationToken ct);
    Task Write(T criterion, string streamName, CancellationToken ct);
    Task Write(IEnumerable<T> collection, CancellationToken ct);
    Task Write(IEnumerable<T> collection, string streamName, CancellationToken ct);
}
