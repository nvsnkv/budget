using CommandLine;
using CommandLine.Text;
using FluentResults;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Accounts;

[Verb("acc", HelpText = "Accounts handling")]
internal class AccountsVerb() : SuperVerb([typeof(AccountStatisticsVerb),typeof(MergeAccountsVerb)]);
