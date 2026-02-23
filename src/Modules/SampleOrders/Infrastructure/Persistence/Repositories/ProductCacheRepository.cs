using Microsoft.EntityFrameworkCore;
using Modules.SampleOrders.Domain.ProductsCache;
using Modules.SampleOrders.Presentation.IntegrationEvents;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.SampleOrders.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository for ProductCache that implements both read and write interfaces.
/// Inherits read operations from CacheReadRepository, adds custom queries and write operations.
/// </summary>
internal sealed class ProductCacheRepository(OrdersDbContext dbContext)
    : CacheReadRepository<ProductCache, int, OrdersDbContext>(dbContext),
      IProductCacheRepository,
      IProductCacheWriter
{
    public override async Task<IReadOnlyCollection<ProductCache>> GetAllAsync(
        int? limit = 100,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ProductCache> query = DbSet.OrderBy(p => p.Name);

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductCache>> GetActiveProductsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Upserts a ProductCache entry. Used only by integration event handlers.
    /// </summary>
    public async Task UpsertAsync(ProductCache productCache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet.FindAsync([productCache.Id], cancellationToken);

        if (existing is null)
        {
            DbSet.Add(productCache);
        }
        else
        {
            existing.Name = productCache.Name;
            existing.Description = productCache.Description;
            existing.Price = productCache.Price;
            existing.IsActive = productCache.IsActive;
            existing.LastSyncedAtUtc = productCache.LastSyncedAtUtc;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
