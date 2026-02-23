using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.Cdc;
using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.PartiesCache;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Sales.Infrastructure.Persistence;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options)
    : ModuleDbContext<SalesDbContext>(options), IUnitOfWork<ISalesModule>
{
    protected override string Schema => Schemas.Sales;

    // Core domain
    internal DbSet<Sale> Sales => Set<Sale>();
    internal DbSet<Package> Packages => Set<Package>();
    internal DbSet<PackageLine> PackageLines => Set<PackageLine>();
    internal DbSet<DeliveryAddress> DeliveryAddresses => Set<DeliveryAddress>();
    internal DbSet<RetailLocation> RetailLocations => Set<RetailLocation>();

    // CDC reference data (Pattern A — authoritative)
    internal DbSet<CdcTaxExemption> CdcTaxExemptions => Set<CdcTaxExemption>();
    internal DbSet<CdcTaxQuestion> CdcTaxQuestions => Set<CdcTaxQuestion>();
    internal DbSet<CdcTaxQuestionText> CdcTaxQuestionTexts => Set<CdcTaxQuestionText>();
    internal DbSet<CdcTaxCalculationError> CdcTaxCalculationErrors => Set<CdcTaxCalculationError>();
    internal DbSet<CdcTaxAllowancePosition> CdcTaxAllowancePositions => Set<CdcTaxAllowancePosition>();
    internal DbSet<CdcProjectCostCategory> CdcProjectCostCategories => Set<CdcProjectCostCategory>();
    internal DbSet<CdcProjectCostItem> CdcProjectCostItems => Set<CdcProjectCostItem>();
    internal DbSet<CdcProjectCostStateMatrix> CdcProjectCostStateMatrices => Set<CdcProjectCostStateMatrix>();
    internal DbSet<CdcPricingHomeMultiplier> CdcPricingHomeMultipliers => Set<CdcPricingHomeMultiplier>();
    internal DbSet<CdcPricingHomeOptionWhitelist> CdcPricingHomeOptionWhitelists => Set<CdcPricingHomeOptionWhitelist>();

    // ECST caches (ICacheProjection)
    internal DbSet<PartyCache> PartiesCache => Set<PartyCache>();
    internal DbSet<PartyPersonCache> PartyPersonsCache => Set<PartyPersonCache>();
    internal DbSet<PartyOrganizationCache> PartyOrganizationsCache => Set<PartyOrganizationCache>();
    internal DbSet<AuthorizedUserCache> AuthorizedUsersCache => Set<AuthorizedUserCache>();
    internal DbSet<FundingRequestCache> FundingRequestsCache => Set<FundingRequestCache>();
    internal DbSet<OnLotHomeCache> OnLotHomesCache => Set<OnLotHomeCache>();
    internal DbSet<LandParcelCache> LandParcelsCache => Set<LandParcelCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
    }
}
