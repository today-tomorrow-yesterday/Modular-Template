using Modules.Organization.Domain.Zones;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Organization.Infrastructure.Persistence.Repositories;

internal sealed class ZoneRepository(OrganizationDbContext dbContext)
    : CacheReadRepository<Zone, int, OrganizationDbContext>(dbContext),
      IZoneRepository;
