using CommandLine;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("ops", false, ["o"], HelpText = "Operations handling")]
internal class OperationsVerb() : SuperVerb([typeof(ImportVerb), typeof(ListVerb)]);
