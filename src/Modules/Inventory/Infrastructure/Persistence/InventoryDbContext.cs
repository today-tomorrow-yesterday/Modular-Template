using Microsoft.EntityFrameworkCore;
using Modules.Inventory.Domain;
using Modules.Inventory.Domain.HomeCentersCache;
using Modules.Inventory.Domain.LandCosts;
using Modules.Inventory.Domain.LandParcels;
using Modules.Inventory.Domain.OnLotHomes;
using Modules.Inventory.Domain.SaleSummariesCache;
using Modules.Inventory.Domain.WheelsAndAxles;
using Modules.Inventory.Infrastructure.Persistence.Configurations;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : ModuleDbContext<InventoryDbContext>(options), IUnitOfWork<IInventoryModule>
{
    protected override string Schema => Schemas.Inventories;

    internal DbSet<OnLotHome> OnLotHomes => Set<OnLotHome>();
    internal DbSet<LandParcel> LandParcels => Set<LandParcel>();
    internal DbSet<LandCost> LandCosts => Set<LandCost>();
    internal DbSet<Domain.AncillaryData.AncillaryData> AncillaryData => Set<Domain.AncillaryData.AncillaryData>();
    internal DbSet<WheelsAndAxlesTransaction> WheelsAndAxlesTransactions => Set<WheelsAndAxlesTransaction>();
    internal DbSet<SaleSummaryCache> SaleSummariesCache => Set<SaleSummaryCache>();
    internal DbSet<HomeCenterCache> HomeCentersCache => Set<HomeCenterCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new OnLotHomeConfiguration());
        modelBuilder.ApplyConfiguration(new LandParcelConfiguration());
        modelBuilder.ApplyConfiguration(new LandCostConfiguration());
        modelBuilder.ApplyConfiguration(new AncillaryDataConfiguration());
        modelBuilder.ApplyConfiguration(new WheelsAndAxlesTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new SaleSummaryCacheConfiguration());
        modelBuilder.ApplyConfiguration(new HomeCenterCacheConfiguration());
    }
}
