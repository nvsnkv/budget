using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NMoneys;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Storage.Context;
using NVs.Budget.Infrastructure.Storage.Entities;

namespace NVs.Budget.Infrastructure.Storage.Repositories;

internal class ExchangeRatesRepository(BudgetContext context, IMapper mapper) : IExchangeRatesRepository
{
    public async Task<ExchangeRate?> GetRate(Owner owner, DateTime asOf, Currency from, Currency to, CancellationToken ct)
    {
        Expression<Func<StoredRate, bool>> criteria = r => r.Owner.Id == owner.Id
                                                           && r.From == from.IsoCode && r.To == to.IsoCode
                                                           && r.AsOf >= r.AsOf.Date && r.AsOf <= asOf;

        var rate = await context.Rates.Where(criteria).OrderByDescending(r => r.AsOf).FirstOrDefaultAsync(ct);
        return mapper.Map<ExchangeRate?>(rate);
    }

    public async Task Add(ExchangeRate rate, Owner owner, CancellationToken ct)
    {
        var storedOwner = await context.Owners.FirstOrDefaultAsync(o => o.Id == owner.Id, ct) ?? throw new InvalidOperationException("Owner is not registered yet! Register owner first!");
        await context.Rates.AddAsync(new(rate.AsOf, rate.From.IsoCode, rate.To.IsoCode, rate.Rate)
        {
            Owner = storedOwner
        }, ct);

        await context.SaveChangesAsync(ct);
    }
}
