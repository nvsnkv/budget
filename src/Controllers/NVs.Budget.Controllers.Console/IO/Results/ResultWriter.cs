using FluentResults;
using Microsoft.Extensions.Options;

namespace NVs.Budget.Controllers.Console.IO.Results;

internal class ResultWriter(OutputStreams outputStreams, IOptions<OutputOptions> options) : GenericResultWriter<Result>(outputStreams, options);
