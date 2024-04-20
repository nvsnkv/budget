using CsvHelper;
using FluentResults;
using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Controllers.Console.IO.Converters;
using NVs.Budget.Controllers.Console.IO.Input.CsvTransfersReader.Errors;
using NVs.Budget.Controllers.Console.IO.Input.Errors;
using NVs.Budget.Controllers.Console.IO.Output.Operations;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Controllers.Console.IO.Input.CsvTransfersReader;

internal class TransferRowReader(IReader parser, CancellationToken cancellationToken, IOperationsRepository repository) : RowParser<UnregisteredTransfer, CsvTransfer>(parser, cancellationToken)
{
    protected override async Task<Result<UnregisteredTransfer>> Convert(CsvTransfer row)
    {
        Guid[] ids = [row.SourceId, row.SinkId];
        var ops = await repository.Get(o => ids.Contains(o.Id), CancellationToken);
        var dict = ops.ToDictionary(o => o.Id);

        if (!dict.TryGetValue(row.SourceId, out TrackedOperation? source))
        {
            return Result.Fail(new SourceNotFoundError(row.SourceId));
        }

        if (!dict.TryGetValue(row.SinkId, out TrackedOperation? sink))
        {
            return Result.Fail(new SinkNotFoundError(row.SinkId));
        }

        Money fee;
        try
        {
            fee = MoneyConverter.Instance.Convert(row.Fee ?? string.Empty, null);
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(Row, [new AttributeParsingError(nameof(row.Fee)), new ExceptionBasedError(e)]));
        }

        return new UnregisteredTransfer(source, sink, fee, row.Comment ?? string.Empty, row.Accuracy);
    }
}