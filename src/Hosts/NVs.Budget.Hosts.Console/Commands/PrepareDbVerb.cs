using CommandLine;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Hosts.Console.Commands;

[Verb("migrate-db", HelpText = "Perform database migration")]
internal class PrepareDbVerb : AbstractVerb;
