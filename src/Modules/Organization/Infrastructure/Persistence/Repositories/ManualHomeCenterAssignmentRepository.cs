using Microsoft.EntityFrameworkCore;
using Modules.Organization.Domain.ManualAssignments;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Organization.Infrastructure.Persistence.Repositories;

internal sealed class ManualHomeCenterAssignmentRepository(OrganizationDbContext dbContext)
    : CacheReadRepository<ManualHomeCenterAssignment, int, OrganizationDbContext>(dbContext),
      IManualHomeCenterAssignmentRepository
{
    public async Task<IReadOnlyCollection<ManualHomeCenterAssignment>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
