using AutoMapper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Models;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

internal class TrackedOperationsWriter(IOutputStreamProvider streams, IOptions<OutputOptions> options, IMapper mapper, CsvConfiguration config)
    : CsvObjectWriter<TrackedOperation, CsvTrackedOperation, CsvTrackedOperationClassMap>(streams, options, mapper, config);
