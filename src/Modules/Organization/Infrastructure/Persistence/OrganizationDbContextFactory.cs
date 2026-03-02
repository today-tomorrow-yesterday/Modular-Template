using Microsoft.EntityFrameworkCore;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Organization.Infrastructure.Persistence;

public sealed class OrganizationDbContextFactory : DesignTimeDbContextFactoryBase<OrganizationDbContext>
{
    protected override OrganizationDbContext CreateContext(DbContextOptions<OrganizationDbContext> options) => new(options);
}
