using FluentResults;
using Microsoft.Extensions.Options;

namespace NVs.Budget.Controllers.Console.IO;

class SimpleResultWriter(OutputStreams outputStreams, IOptions<OutputOptions> options) : ResultWriter<Result>(outputStreams, options);
