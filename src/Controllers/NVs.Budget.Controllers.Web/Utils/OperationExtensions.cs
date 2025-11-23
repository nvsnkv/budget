using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Controllers.Web.Models;

namespace NVs.Budget.Controllers.Web.Utils;

public static class OperationExtensions
{
    /// <summary>
    /// Orders operations by timestamp in descending order (newest first)
    /// </summary>
    public static IOrderedEnumerable<OperationResponse> OrderHistorically(this IEnumerable<OperationResponse> operations)
    {
        return operations.OrderByDescending(o => o.Timestamp);
    }

    /// <summary>
    /// Orders operations by timestamp in descending order (newest first)
    /// </summary>
    public static IOrderedAsyncEnumerable<TrackedOperation> OrderHistorically(this IAsyncEnumerable<TrackedOperation> operations)
    {
        return operations.OrderByDescending(o => o.Timestamp);
    }

    /// <summary>
    /// Orders groups of operations by the timestamp of the first operation in each group (newest first)
    /// </summary>
    public static IOrderedEnumerable<IReadOnlyCollection<OperationResponse>> OrderHistorically(
        this IEnumerable<IReadOnlyCollection<OperationResponse>> groups)
    {
        return groups.OrderByDescending(group => group.FirstOrDefault()?.Timestamp ?? DateTime.MinValue);
    }
}

