using AutoMapper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.IO.Output.Operations;

namespace NVs.Budget.Controllers.Console.IO.Output.Accounts;

internal class TrackedAccountsWriter(IOutputStreamProvider streams, IOptionsSnapshot<OutputOptions> options, IMapper mapper, CsvConfiguration config)
:CsvObjectWriter<TrackedAccount, CsvAccount, CsvAccountClassMap>(streams, options, mapper, config);
