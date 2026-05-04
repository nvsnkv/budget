using NMoneys;
using NVs.Budget.Domain.Entities.Plans;

namespace NVs.Budget.Application.Contracts.Entities.Planning;

public class TrackedPlanExpectation(
    Guid id,
    Money expectedAmount,
    DateTime from,
    DateTime till,
    string? subcriterionPath = null,
    string? note = null)
    : PlanExpectation(id, expectedAmount, from, till, subcriterionPath, note);
