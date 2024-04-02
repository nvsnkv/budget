using CommandLine;
using MediatR;

namespace NVs.Budget.Controllers.Console.Commands;

internal abstract class SuperVerb(Type[] verbs) : IRequest<int>
{
    public Type[] Verbs { get; } = verbs;

    [Value(0)]
    public IEnumerable<string>? Args { get; set; }
}
