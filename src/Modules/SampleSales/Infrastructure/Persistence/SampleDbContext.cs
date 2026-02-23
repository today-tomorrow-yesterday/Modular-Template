using Microsoft.EntityFrameworkCore;
using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Catalogs;
using Modules.SampleSales.Domain.OrdersCache;
using Modules.SampleSales.Domain.Products;
using Modules.SampleSales.Infrastructure.Persistence.Configurations;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.SampleSales.Infrastructure.Persistence;

public sealed class SampleDbContext(DbContextOptions<SampleDbContext> options)
    : ModuleDbContext<SampleDbContext>(options), IUnitOfWork<ISampleSalesModule>
{
    protected override string Schema => Schemas.Sample;

    internal DbSet<Product> Products => Set<Product>();
    internal DbSet<Catalog> Catalogs => Set<Catalog>();
    internal DbSet<CatalogProduct> CatalogProducts => Set<CatalogProduct>();
    internal DbSet<OrderCache> OrdersCache => Set<OrderCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogProductConfiguration());
        modelBuilder.ApplyConfiguration(new OrderCacheConfiguration());
    }
}
