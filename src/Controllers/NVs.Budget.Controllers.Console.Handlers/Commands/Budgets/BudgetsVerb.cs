using CommandLine;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Budgets;

[Verb("budget", HelpText = "Budgets handling")]
internal class BudgetsVerb() : SuperVerb([typeof(ListVerb), typeof(DetailsVerb), typeof(AddVerb), typeof(MergeVerb), typeof(UpdateVerb)]);
