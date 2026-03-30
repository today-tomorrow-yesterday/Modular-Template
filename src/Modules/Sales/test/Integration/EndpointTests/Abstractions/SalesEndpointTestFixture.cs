using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Domain.Cdc;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Sales;
using Modules.Sales.Infrastructure.Persistence;
using Modules.Sales.Integration.Shared;
using Rtl.Core.Application.Caching;

namespace Modules.Sales.EndpointTests.Abstractions;

// Test fixture for Sales API endpoint tests (CreateSale, UpdatePackageHome, etc.).
//
// Provides helpers to seed cache data that certain endpoints depend on —
// for example, SeedLandParcelCacheAsync populates inventory cache entries
// needed by the UpdatePackageLand handler's HomeCenterOwnedLand lookup.
public class SalesEndpointTestFixture : SalesTestFixtureBase
{
    // ── Cache seeding helpers ──────────────────────────────────────

    /// <summary>
    /// Seeds a LandParcelCache entry for tests that need HomeCenterOwnedLand lookup.
    /// </summary>
    public async Task SeedLandParcelCacheAsync(string stockNumber, decimal landCost)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var cacheWriteScope = scope.ServiceProvider.GetRequiredService<ICacheWriteScope>();

        using (cacheWriteScope.AllowWrites())
        {
            db.Set<Domain.InventoryCache.LandParcelCache>().Add(
                new Domain.InventoryCache.LandParcelCache
                {
                    RefPublicId = Guid.CreateVersion7(),
                    RefHomeCenterNumber = TestHomeCenterNumber,
                    RefStockNumber = stockNumber,
                    LandCost = landCost,
                    LastSyncedAtUtc = DateTime.UtcNow
                });
            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds CDC project cost reference data (categories + items) for tests that call
    /// the UpdatePackageProjectCosts endpoint. CDC data lives in the 'cdc' schema which
    /// is not reset by Respawn, so this is idempotent — skips seeding if data already exists.
    /// </summary>
    public async Task SeedCdcProjectCostDataAsync(params (int CategoryNumber, string CategoryDescription, int ItemNumber, string ItemDescription)[] entries)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        foreach (var (catNum, catDesc, itemNum, itemDesc) in entries)
        {
            // Find or create the category
            var category = await db.Set<CdcProjectCostCategory>()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.MasterDealer == 29 && c.CategoryNumber == catNum);

            if (category is null)
            {
                category = new CdcProjectCostCategory
                {
                    MasterDealer = 29,
                    CategoryNumber = catNum,
                    Description = catDesc,
                    CreatedAtUtc = DateTime.UtcNow
                };
                db.Set<CdcProjectCostCategory>().Add(category);
                await db.SaveChangesAsync();
            }

            // Check if item already exists
            var itemExists = category.Items.Any(i => i.ItemNumber == itemNum);
            if (!itemExists)
            {
                db.Set<CdcProjectCostItem>().Add(new CdcProjectCostItem
                {
                    MasterDealer = 29,
                    ProjectCostCategoryId = category.Id,
                    CategoryId = catNum,
                    ItemNumber = itemNum,
                    Description = itemDesc,
                    Status = "A",
                    CreatedAtUtc = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Seeds a FundingRequestCache entry for tests that require AppId (tax, commission).
    /// Looks up internal Sale and Package IDs from the public GUIDs.
    /// </summary>
    public async Task SeedFundingRequestCacheAsync(Guid salePublicId, Guid packagePublicId, int appId = 999999)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var cacheWriteScope = scope.ServiceProvider.GetRequiredService<ICacheWriteScope>();

        var sale = await db.Set<Sale>().FirstAsync(s => s.PublicId == salePublicId);
        var package = await db.Set<Package>().FirstAsync(p => p.PublicId == packagePublicId);

        using (cacheWriteScope.AllowWrites())
        {
            db.Set<FundingRequestCache>().Add(new FundingRequestCache
            {
                Id = Random.Shared.Next(9000, 99999),
                RefFundingRequestId = Random.Shared.Next(9000, 99999),
                SaleId = sale.Id,
                PackageId = package.Id,
                FundingKeys = JsonDocument.Parse($$"""[{"Key":"AppId","Value":"{{appId}}"}]"""),
                LenderId = 1,
                LenderName = "Test Lender",
                Status = FundingRequestStatus.Approved,
                RequestAmount = 80_000m,
                LastSyncedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }
}
