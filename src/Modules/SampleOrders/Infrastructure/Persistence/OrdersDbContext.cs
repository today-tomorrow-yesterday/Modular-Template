using Microsoft.EntityFrameworkCore;
using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.ProductsCache;
using Modules.SampleOrders.Infrastructure.Persistence.Configurations;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.SampleOrders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options)
    : ModuleDbContext<OrdersDbContext>(options), IUnitOfWork<ISampleOrdersModule>
{
    protected override string Schema => Schemas.Orders;

    internal DbSet<Order> Orders => Set<Order>();
    internal DbSet<OrderLine> OrderLines => Set<OrderLine>();
    internal DbSet<Customer> Customers => Set<Customer>();
    internal DbSet<ProductCache> ProductsCache => Set<ProductCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCacheConfiguration());
    }
}
