using CommandLine;
using JetBrains.Annotations;
using NVs.Budget.Controllers.Console.Contracts;
using NVs.Budget.Controllers.Console.Contracts.Commands;

namespace NVs.Budget.Controllers.Console.Commands.Owners;

[Verb("owners", HelpText = "Owners management"), UsedImplicitly]
internal class OwnersVerb() : SuperVerb([typeof(ListOwnersVerb), typeof(SelfRegisterVerb)]);
