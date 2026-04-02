using Microsoft.EntityFrameworkCore;
using Modules.SampleSales.Domain.OrdersCache;
using ModularTemplate.Infrastructure.Caching;

namespace Modules.SampleSales.Infrastructure.Persistence.Repositories;

internal sealed class OrderCacheRepository(SampleDbContext dbContext)
    : CacheReadRepository<OrderCache, SampleDbContext>(dbContext),
      IOrderCacheRepository,
      IOrderCacheWriter
{
    public override async Task<IReadOnlyCollection<OrderCache>> GetAllAsync(
        int? limit = 100,
        CancellationToken cancellationToken = default)
    {
        IQueryable<OrderCache> query = DbSet.OrderByDescending(o => o.OrderedAtUtc);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<OrderCache?> GetByRefPublicIdAsync(
        Guid refPublicId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(o => o.RefPublicId == refPublicId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<OrderCache>> GetByCustomerIdAsync(
        Guid publicCustomerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.RefPublicCustomerId == publicCustomerId)
            .OrderByDescending(o => o.OrderedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(OrderCache orderCache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(o => o.RefPublicId == orderCache.RefPublicId, cancellationToken);

        if (existing is null)
        {
            DbSet.Add(orderCache);
        }
        else
        {
            existing.RefPublicCustomerId = orderCache.RefPublicCustomerId;
            existing.TotalPrice = orderCache.TotalPrice;
            existing.Currency = orderCache.Currency;
            existing.Status = orderCache.Status;
            existing.OrderedAtUtc = orderCache.OrderedAtUtc;
            existing.LastSyncedAtUtc = orderCache.LastSyncedAtUtc;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
