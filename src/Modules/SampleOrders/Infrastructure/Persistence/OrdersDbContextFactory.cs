using Microsoft.EntityFrameworkCore;
using ModularTemplate.Infrastructure.Persistence;

namespace Modules.SampleOrders.Infrastructure.Persistence;

public sealed class OrdersDbContextFactory : DesignTimeDbContextFactoryBase<OrdersDbContext>
{
    protected override OrdersDbContext CreateContext(DbContextOptions<OrdersDbContext> options) => new(options);
}
