using Modules.Organization.Domain.Regions;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Organization.Infrastructure.Persistence.Repositories;

internal sealed class RegionRepository(OrganizationDbContext dbContext)
    : CacheReadRepository<Region, int, OrganizationDbContext>(dbContext),
      IRegionRepository;
