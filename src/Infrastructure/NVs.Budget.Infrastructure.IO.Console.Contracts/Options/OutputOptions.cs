namespace NVs.Budget.Infrastructure.IO.Console.Options;

public class OutputOptions
{
    public bool ShowSuccesses { get; set; }

    public string OutputStreamName { get; set; } = string.Empty;

    public string ErrorStreamName { get; set; } = string.Empty;
}
