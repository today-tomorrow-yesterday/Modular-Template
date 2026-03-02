using Microsoft.EntityFrameworkCore;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Sales.Infrastructure.Persistence;

public sealed class SalesDbContextFactory : DesignTimeDbContextFactoryBase<SalesDbContext>
{
    protected override SalesDbContext CreateContext(DbContextOptions<SalesDbContext> options) => new(options);
}
