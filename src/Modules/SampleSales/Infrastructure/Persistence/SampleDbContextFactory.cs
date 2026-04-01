using Microsoft.EntityFrameworkCore;
using ModularTemplate.Infrastructure.Persistence;

namespace Modules.SampleSales.Infrastructure.Persistence;

public sealed class SampleDbContextFactory : DesignTimeDbContextFactoryBase<SampleDbContext>
{
    protected override SampleDbContext CreateContext(DbContextOptions<SampleDbContext> options) => new(options);
}
