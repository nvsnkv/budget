using CommandLine;
using JetBrains.Annotations;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Budgets;

internal class BudgetDetailsVerb : AbstractVerb
{
    private string _budgetId = string.Empty;

    [Value(0, Required = true, MetaName = "Budget id")]
    public string Id
    {
        get => _budgetId;
        [UsedImplicitly] set
        {
            _budgetId = value;
            CsvReadingOptionsPath ??= $"{value}_csv-reading-options.yml";
            LogbookCriteriaPath ??= $"{value}_logbook-criteria.yml";
            TaggingCriteriaPath ??= $"{value}_tagging-criteria.yml";
            TransferCriteriaPath ??= $"{value}_transfer-criteria.yml";
        }
    }

    [Option("csv-reading-options", HelpText = "Path to YAML file with CSV reading options. If not defined, app will use budget-related name")]
    public string? CsvReadingOptionsPath { get; [UsedImplicitly] set; }

    [Option("tagging-criteria", HelpText = "Path to YAML file with tagging criteria.  If not defined, app will use budget-related name")]
    public string? TaggingCriteriaPath { get; [UsedImplicitly] set; }

    [Option("transfer-criteria", HelpText = "Path to YAML file with transfer criteria.  If not defined, app will use budget-related name")]
    public string? TransferCriteriaPath { get; [UsedImplicitly] set; }

    [Option("logbook-criteria", HelpText = "Path to YAML file with logbook criteria. If defined, transfer criteria will be updated")]
    public string? LogbookCriteriaPath { get; [UsedImplicitly] set; }
}
