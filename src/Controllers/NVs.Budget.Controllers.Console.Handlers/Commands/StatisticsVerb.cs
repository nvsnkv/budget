using CommandLine;

namespace NVs.Budget.Controllers.Console.Handlers.Commands;

internal class StatisticsVerb : CriteriaBasedVerb
{
    [Option('p', "logbook-path", Required = true, HelpText = "A path to logbook to write")]
    public string LogbookPath { get; set; } = string.Empty;

    [Option('f', "from", HelpText = "Date from")]
    public DateTime From { get; set; } = DateTime.MinValue;

    [Option('t', "till", HelpText = "Date till")]
    public DateTime Till { get; set; } = DateTime.MaxValue;

    [Option('s', "schedule", HelpText = "Cron expression to generate time ranges. If not set, all values will be accumulated in a single time range between From and Till")]
    public string? Schedule { get; set; }

    [Option("with-counts", Default = true, HelpText = "Write operations count for each account")]
    public bool WithCounts { get; set; }

    [Option("with-amount", HelpText = "Write amounts for each account")]
    public bool WithAmounts { get; set; }

    [Option("with-operations", HelpText = "Write list of operations for each account")]
    public bool WithOperations { get; set; }
}
