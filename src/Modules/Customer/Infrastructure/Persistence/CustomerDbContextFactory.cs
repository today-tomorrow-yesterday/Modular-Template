using Microsoft.EntityFrameworkCore;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Customer.Infrastructure.Persistence;

public sealed class CustomerDbContextFactory : DesignTimeDbContextFactoryBase<CustomerDbContext>
{
    protected override CustomerDbContext CreateContext(DbContextOptions<CustomerDbContext> options) => new(options);
}
