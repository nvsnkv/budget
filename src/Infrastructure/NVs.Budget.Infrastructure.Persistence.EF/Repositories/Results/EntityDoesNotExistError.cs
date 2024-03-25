namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

class EntityDoesNotExistError<T>(T entity) : ErrorBase("Entity does not exist!", new Dictionary<string, object>
{
    { nameof(entity), entity! }
});
