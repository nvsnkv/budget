using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

internal class TrackedOperationsWriter(IOutputStreamProvider streams, IOptions<OutputOptions> options, IMapper mapper, CsvConfiguration config)
    : CsvObjectWriter<TrackedOperation, CsvTrackedOperation, CsvTrackedOperationClassMap>(streams, options, mapper, config);

internal class OperationsWriter(IOutputStreamProvider streams, IOptions<OutputOptions> options, IMapper mapper, CsvConfiguration config)
    : CsvObjectWriter<Operation, CsvOperation, CsvOperationClassMap>(streams, options, mapper, config);
