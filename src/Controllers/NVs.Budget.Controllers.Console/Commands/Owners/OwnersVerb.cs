using CommandLine;

namespace NVs.Budget.Controllers.Console.Commands.Owners;

[Verb("owners", HelpText = "Owners management")]
internal class OwnersVerb() : SuperVerb([typeof(ListOwnersVerb), typeof(SelfRegisterVerb)]);
