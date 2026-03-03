using Microsoft.EntityFrameworkCore;
using Modules.Funding.Domain.CustomersCache;
using Modules.Funding.Presentation.IntegrationEvents;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Funding.Infrastructure.Persistence.Repositories;

internal sealed class CustomerCacheRepository(FundingDbContext dbContext)
    : CacheReadRepository<CustomerCache, int, FundingDbContext>(dbContext),
      ICustomerCacheRepository,
      ICustomerCacheWriter
{
    public async Task UpsertAsync(CustomerCache customerCache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == customerCache.RefPublicId, cancellationToken);

        if (existing is null)
        {
            DbSet.Add(customerCache);
        }
        else
        {
            existing.LoanId = customerCache.LoanId;
            existing.FirstName = customerCache.FirstName;
            existing.LastName = customerCache.LastName;
            existing.HomeCenterNumber = customerCache.HomeCenterNumber;
            existing.LastSyncedAtUtc = customerCache.LastSyncedAtUtc;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateNameAsync(Guid refPublicId, string firstName, string lastName, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null)
        {
            return;
        }

        existing.FirstName = firstName;
        existing.LastName = lastName;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateHomeCenterAsync(Guid refPublicId, int homeCenterNumber, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(c => c.RefPublicId == refPublicId, cancellationToken);

        if (existing is null)
        {
            return;
        }

        existing.HomeCenterNumber = homeCenterNumber;
        existing.LastSyncedAtUtc = lastSyncedAtUtc;

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
