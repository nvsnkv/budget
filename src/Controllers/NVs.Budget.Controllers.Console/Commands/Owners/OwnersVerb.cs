using CommandLine;
using MediatR;

namespace NVs.Budget.Controllers.Console.Commands.Owners;

[Verb("owners")]
internal class OwnersVerb() : SuperVerb([typeof(ListOwnersVerb)])
{
    
}

[Verb("list")]
internal class ListOwnersVerb : IRequest<int> { }
