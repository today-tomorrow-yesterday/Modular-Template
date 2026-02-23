using Microsoft.EntityFrameworkCore;
using Modules.Organization.Domain.Users;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Organization.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(OrganizationDbContext dbContext)
    : CacheReadRepository<User, int, OrganizationDbContext>(dbContext),
      IUserRepository
{
    public async Task<User?> GetByFederatedIdAsync(
        string federatedId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.FederatedId == federatedId, cancellationToken);
    }

    public async Task<User?> GetByEmployeeNumberAsync(
        int employeeNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.EmployeeNumber == employeeNumber, cancellationToken);
    }
}
