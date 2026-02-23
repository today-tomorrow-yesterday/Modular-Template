using Microsoft.EntityFrameworkCore;
using Modules.SampleSales.Domain.OrdersCache;
using Modules.SampleSales.Presentation.IntegrationEvents;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.SampleSales.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for OrderCache that implements both read and write interfaces.
/// Inherits read operations from CacheReadRepository, adds custom queries and write operations.
/// </summary>
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

    public async Task<IReadOnlyCollection<OrderCache>> GetByCustomerIdAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Upserts an OrderCache entry. Used only by integration event handlers.
    /// </summary>
    public async Task UpsertAsync(OrderCache orderCache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet.FindAsync([orderCache.Id], cancellationToken);

        if (existing is null)
        {
            DbSet.Add(orderCache);
        }
        else
        {
            existing.CustomerId = orderCache.CustomerId;
            existing.TotalPrice = orderCache.TotalPrice;
            existing.Currency = orderCache.Currency;
            existing.Status = orderCache.Status;
            existing.OrderedAtUtc = orderCache.OrderedAtUtc;
            existing.LastSyncedAtUtc = orderCache.LastSyncedAtUtc;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
