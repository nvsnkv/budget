using NVs.Budget.Application.Contracts.Entities;

namespace NVs.Budget.Infrastructure.Storage.Repositories.Results;

class VersionDoesNotMatchError<T, TId>(T entity) : ErrorBase("Version of entity differs from recorded entity!", new()
{
    { nameof(entity.Id), entity.Id }
}) where T : ITrackableEntity<TId> where TId : struct;