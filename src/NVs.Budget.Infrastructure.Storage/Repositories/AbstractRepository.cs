using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Storage.Context;
using NVs.Budget.Infrastructure.Storage.Entities;
using NVs.Budget.Infrastructure.Storage.Repositories.Results;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Storage.Repositories;

internal abstract class AbstractRepository<T, TStored> where T: class where TStored: class
{
    private readonly IMapper _mapper;
    private readonly BudgetContext _context;
    private readonly Func<BudgetContext, DbSet<TStored>> _selector;

    protected AbstractRepository(IMapper mapper, BudgetContext context, Func<BudgetContext, DbSet<TStored>> selector)
    {
        _mapper = mapper;
        _context = context;
        _selector = selector;
    }

    public async Task<Result<T>> Update(T entity, CancellationToken ct)
    {
        var updated = _selector(_context).Persist(_mapper).InsertOrUpdateAsync(entity, ct);
        if (_context.ChangeTracker.Entries<TStored>().Any(e => e.State == EntityState.Added))
        {
            return Result.Fail(new EntityDoesNotExistError<T>(entity));
        }

        await _context.SaveChangesAsync(ct);
        return _mapper.Map<T>(updated);
    }

    protected async Task<Result<T>> DoInsert(TStored newEntity, CancellationToken ct)
    {
        await _selector(_context).AddAsync(newEntity, ct);
        await _context.SaveChangesAsync(ct);

        return _mapper.Map<T>(newEntity);
    }

    public async Task<Result> Remove(T entity, CancellationToken ct)
    {
        await _selector(_context).Persist(_mapper).RemoveAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyCollection<T>> Get(Expression<Func<T, bool>> filter, CancellationToken ct)
    {
        var converted = ConvertExpression(filter);
        var query = _mapper.ProjectTo<T>(_selector(_context).Where(converted));
        return await query.ToListAsync(ct);
    }

    protected virtual Expression<Func<TStored, bool>> ConvertExpression(Expression<Func<T, bool>> source)
        => source.ConvertTypes<T, TStored>(MappingProfile.TypeMappings);
}
