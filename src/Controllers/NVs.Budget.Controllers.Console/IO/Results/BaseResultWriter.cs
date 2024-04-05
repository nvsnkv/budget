using FluentResults;
using Microsoft.Extensions.Options;

namespace NVs.Budget.Controllers.Console.IO.Results;

internal class BaseResultWriter(OutputStreams outputStreams, IOptions<OutputOptions> options) : GenericResultWriter<IResultBase>(outputStreams, options);
