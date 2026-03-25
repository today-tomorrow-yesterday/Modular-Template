using Microsoft.Extensions.DependencyInjection;
using Modules.Sales.Infrastructure.Persistence;
using Modules.Sales.Integration.Shared;
using Rtl.Core.Application.Caching;

namespace Modules.Sales.EndpointTests.Abstractions;

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
            db.Set<Modules.Sales.Domain.InventoryCache.LandParcelCache>().Add(
                new Modules.Sales.Domain.InventoryCache.LandParcelCache
                {
                    RefLandParcelId = Random.Shared.Next(9000, 99999),
                    RefHomeCenterNumber = TestHomeCenterNumber,
                    RefStockNumber = stockNumber,
                    LandCost = landCost,
                    LastSyncedAtUtc = DateTime.UtcNow
                });
            await db.SaveChangesAsync();
        }
    }
}
