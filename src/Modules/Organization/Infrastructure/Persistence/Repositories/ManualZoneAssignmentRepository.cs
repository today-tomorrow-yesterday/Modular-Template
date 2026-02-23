using Microsoft.EntityFrameworkCore;
using Modules.Organization.Domain.ManualAssignments;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Organization.Infrastructure.Persistence.Repositories;

internal sealed class ManualZoneAssignmentRepository(OrganizationDbContext dbContext)
    : CacheReadRepository<ManualZoneAssignment, int, OrganizationDbContext>(dbContext),
      IManualZoneAssignmentRepository
{
    public async Task<IReadOnlyCollection<ManualZoneAssignment>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
