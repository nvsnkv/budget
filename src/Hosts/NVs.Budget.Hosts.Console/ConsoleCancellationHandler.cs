namespace NVs.Budget.Hosts.Console;

public class ConsoleCancellationHandler
{
    private readonly CancellationTokenSource _source = new();

    public CancellationToken Token => _source.Token;

    public ConsoleCancellationHandler()
    {
        System.Console.CancelKeyPress += (_, _) =>
        {
            _source.Cancel(true);
        };
    }
}
