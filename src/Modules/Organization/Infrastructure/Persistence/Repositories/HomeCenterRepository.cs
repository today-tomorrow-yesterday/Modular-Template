using Microsoft.EntityFrameworkCore;
using Modules.Organization.Domain.HomeCenters;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Organization.Infrastructure.Persistence.Repositories;

internal sealed class HomeCenterRepository(OrganizationDbContext dbContext)
    : CacheReadRepository<HomeCenter, int, OrganizationDbContext>(dbContext),
      IHomeCenterRepository
{
    public async Task<HomeCenter?> GetByHomeCenterNumberAsync(
        int homeCenterNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.RefHomeCenterNumber == homeCenterNumber, cancellationToken);
    }
}
