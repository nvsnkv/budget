using CommandLine;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Test;

[Verb("test", HelpText = "Configuration testing")]
internal class TestVerb() : SuperVerb([typeof(TestImportVerb), typeof(TestOperationStatsStatisticsVerb)]);
