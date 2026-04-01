using Microsoft.EntityFrameworkCore;
using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.ProductsCache;
using Modules.SampleOrders.Infrastructure.Persistence.Configurations;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Infrastructure.Persistence;

namespace Modules.SampleOrders.Infrastructure.Persistence;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options)
    : ModuleDbContext<OrdersDbContext>(options), IUnitOfWork<ISampleOrdersModule>
{
    protected override string Schema => Schemas.Orders;

    internal DbSet<Order> Orders => Set<Order>();
    internal DbSet<OrderLine> OrderLines => Set<OrderLine>();
    internal DbSet<Customer> Customers => Set<Customer>();
    internal DbSet<CustomerContact> CustomerContacts => Set<CustomerContact>();
    internal DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    internal DbSet<ShippingAddress> ShippingAddresses => Set<ShippingAddress>();
    internal DbSet<ProductCache> ProductsCache => Set<ProductCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderLineConfiguration());
        modelBuilder.ApplyConfiguration(new ProductLineConfiguration());
        modelBuilder.ApplyConfiguration(new CustomLineConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerContactConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerAddressConfiguration());
        modelBuilder.ApplyConfiguration(new ShippingAddressConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCacheConfiguration());
    }
}
