using CsvHelper;
using FluentResults;
using NVs.Budget.Infrastructure.IO.Console.Input.Errors;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

internal abstract class RowParser<T, TRow>(IReader parser, CancellationToken cancellationToken)
{
    private volatile int _row = -1;

    protected readonly CancellationToken CancellationToken = cancellationToken;

    protected int Row => _row;

    public async Task<bool> ReadAsync()
    {
        CancellationToken.ThrowIfCancellationRequested();

        var result = await parser.ReadAsync();
        if (result)
        {
            Interlocked.Increment(ref _row);
        }

        return result;
    }

    public async Task<Result<T>> GetRecord()
    {
        TRow row;
        try
        {
            row = parser.GetRecord<TRow>();
            if (row is null)
            {
                return Result.Fail(new RowNotParsedError(_row, []));
            }
        }
        catch (Exception e)
        {
            return Result.Fail(new RowNotParsedError(_row, [new ExceptionalError(e)]));
        }

        return await Convert(row);
    }

    protected abstract Task<Result<T>> Convert(TRow row);
}
