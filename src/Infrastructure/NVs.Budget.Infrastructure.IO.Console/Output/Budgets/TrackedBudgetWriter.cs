using AutoMapper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.IO.Console.Models;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Budgets;

internal class TrackedBudgetWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options, IMapper mapper, CsvConfiguration config)
:CsvObjectWriter<TrackedBudget, CsvBudget, CsvBudgetClassMap>(streams, options, mapper, config);
