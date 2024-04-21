using CommandLine;
using MediatR;

namespace NVs.Budget.Controllers.Console.Contracts.Commands;

public abstract class SuperVerb(Type[] verbs) : IRequest<ExitCode>
{
    public Type[] Verbs { get; } = verbs;

    [Option('o', "output", Required = false, HelpText = "Output path. If no value is set, app will use value from configuration")]
    public string? OutputPath { get; set; }

    [Option('e', "errors", Required = false, HelpText = "Errors path. If no value is set, app will use value from configuration")]
    public string? ErrorsPath { get; set; }

    [Value(0)] public IEnumerable<string>? Args { get; set; }
}
