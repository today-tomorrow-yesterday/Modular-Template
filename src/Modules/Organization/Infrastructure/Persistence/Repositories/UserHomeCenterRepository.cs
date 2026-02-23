using Microsoft.EntityFrameworkCore;
using Modules.Organization.Domain.Users;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Organization.Infrastructure.Persistence.Repositories;

internal sealed class UserHomeCenterRepository(OrganizationDbContext dbContext)
    : CacheReadRepository<UserHomeCenter, int, OrganizationDbContext>(dbContext),
      IUserHomeCenterRepository
{
    public async Task<IReadOnlyCollection<UserHomeCenter>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(uhc => uhc.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<UserHomeCenter>> GetByHomeCenterIdAsync(
        int homeCenterId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(uhc => uhc.HomeCenterId == homeCenterId)
            .ToListAsync(cancellationToken);
    }
}
