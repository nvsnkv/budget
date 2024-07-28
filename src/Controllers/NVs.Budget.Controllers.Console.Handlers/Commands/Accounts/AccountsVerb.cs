using CommandLine;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Accounts;

[Verb("acc", HelpText = "Accounts handling")]
internal class AccountsVerb() : SuperVerb([typeof(ListVerb),typeof(AccountStatisticsVerb),typeof(MergeAccountsVerb)]);
