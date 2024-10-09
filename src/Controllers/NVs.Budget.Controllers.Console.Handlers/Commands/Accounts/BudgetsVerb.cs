using CommandLine;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Accounts;

[Verb("budget", HelpText = "Budgets handling")]
internal class BudgetsVerb() : SuperVerb([typeof(ListVerb)]);
