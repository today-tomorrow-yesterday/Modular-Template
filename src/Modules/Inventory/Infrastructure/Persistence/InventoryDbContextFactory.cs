using Microsoft.EntityFrameworkCore;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContextFactory : DesignTimeDbContextFactoryBase<InventoryDbContext>
{
    protected override InventoryDbContext CreateContext(DbContextOptions<InventoryDbContext> options) => new(options);
}
