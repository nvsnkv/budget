using CommandLine;
using MediatR;

namespace NVs.Budget.Controllers.Console.Contracts.Commands;

public abstract class SuperVerb(Type[] verbs) : IRequest<ExitCode>
{
    public Type[] Verbs { get; } = verbs;

    [Value(0)] public IEnumerable<string>? Args { get; set; }
}
