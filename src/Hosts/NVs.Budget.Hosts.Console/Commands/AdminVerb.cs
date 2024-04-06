using CommandLine;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Hosts.Console.Commands;

[Verb("admin", HelpText = "Administrative actions")]
internal class AdminVerb() : SuperVerb([typeof(PrepareDbVerb)])
{

}
