namespace NVs.Budget.Infrastructure.Storage.Repositories.Results;

class EntityDoesNotExistError<T>(T entity) : ErrorBase("Entity does not exist!", new Dictionary<string, object>
{
    { nameof(entity), entity! }
});
