using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class AuthorizedUserCacheRepository(SalesDbContext dbContext)
    : CacheReadRepository<AuthorizedUserCache, int, SalesDbContext>(dbContext),
      IAuthorizedUserCacheRepository,
      IAuthorizedUserCacheWriter
{
    public async Task UpsertAsync(AuthorizedUserCache cache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(u => u.RefUserId == cache.RefUserId, cancellationToken);

        if (existing is null)
        {
            DbSet.Add(cache);
        }
        else
        {
            existing.ApplyChangesFrom(cache);
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthorizedUserCache?> GetByFederatedIdAsync(
        string federatedId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.FederatedId == federatedId, cancellationToken);
    }

    public async Task<AuthorizedUserCache?> GetByEmployeeNumberAsync(
        int employeeNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.EmployeeNumber == employeeNumber, cancellationToken);
    }

    public async Task<bool> AllExistAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0) return true;

        var matchCount = await DbSet
            .AsNoTracking()
            .CountAsync(u => ids.Contains(u.Id) && u.IsActive && !u.IsRetired, cancellationToken);

        return matchCount == ids.Count;
    }
}
