using CommandLine;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Transfers;

[Verb("xfers", false, ["x"], HelpText = "Transfers management")]
internal class TransfersVerb() : SuperVerb([typeof(SearchVerb), typeof(RegisterVerb), typeof(RemoveTransfersVerb)]);
