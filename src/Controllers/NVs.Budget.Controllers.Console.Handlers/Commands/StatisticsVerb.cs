using CommandLine;
using JetBrains.Annotations;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands;

internal class StatisticsVerb : AbstractVerb
{
    [Option('p', "logbook-path", Required = false, HelpText = "A path to logbook to write. If not set app will create a new file with time-based name in a working dir")]
    public string LogbookPath { get; [UsedImplicitly] set; } = $"stats_{DateTime.Now:yyyy-MM-dd_hhmmss}.xlsx";

    [Option('f', "from", Required = false, HelpText = "Date from. If not set app will use beginning of the current year")]
    public DateTime From { get; [UsedImplicitly] set; } = new(new DateOnly(DateTime.Now.Year, 1, 1), new TimeOnly(0, 0, 0), DateTime.Now.Kind);

    [Option('t', "till", Required = false, HelpText = "Date till. If not set app will use current moment")]
    public DateTime Till { get; [UsedImplicitly] set; } = DateTime.Now;

    [Option('s', "schedule", Default = "0 0 1 * *", HelpText = "Cron expression to generate time ranges")]
    public string? Schedule { get; [UsedImplicitly] set; }

    [Option("with-counts", HelpText = "Write operations count for each category")]
    public bool WithCounts { get; [UsedImplicitly] set; }

    [Option("with-operations", HelpText = "Write list of operations for each category")]
    public bool WithOperations { get; [UsedImplicitly] set; }
}
