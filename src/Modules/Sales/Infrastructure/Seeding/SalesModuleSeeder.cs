using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Infrastructure.Persistence;
using Modules.Sales.Infrastructure.Seeding.Fakers;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Seeding;

namespace Modules.Sales.Infrastructure.Seeding;

internal sealed class SalesModuleSeeder : IModuleSeeder
{
    public string ModuleName => "Sales";
    public int Order => 40;

    public async Task SeedAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var db = services.GetRequiredService<SalesDbContext>();
        var cacheWriteScope = services.GetRequiredService<ICacheWriteScope>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger<SalesModuleSeeder>();

        // Idempotency check
        if (await db.Sales.AnyAsync(ct))
        {
            logger.LogInformation("Sales module already has data. Skipping seed.");
            return;
        }

        var faker = new Bogus.Faker();
        var activeHomeCenterNumbers = RetailLocationFaker.GetHomeCenterNumbers();

        // ────────────────────────────────────────
        // Phase 1: Independent entities (no FKs between them)
        // ────────────────────────────────────────

        // RetailLocations (core domain — no cache write scope needed)
        var retailLocations = RetailLocationFaker.Generate();
        foreach (var rl in retailLocations) rl.ClearDomainEvents();
        db.RetailLocations.AddRange(retailLocations);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} retail locations.", retailLocations.Count);

        // Cache entities need write scope
        using (cacheWriteScope.AllowWrites())
        {
            // PartyCache (UseIdentityAlwaysColumn — DB generates Id)
            var partyCacheFaker = new PartyCacheFaker(activeHomeCenterNumbers);
            var parties = partyCacheFaker.Generate(20);
            db.PartiesCache.AddRange(parties);
            await db.SaveChangesAsync(ct); // Must flush to get auto-generated PartyIds

            // PartyPersonCache (ValueGeneratedNever on PartyId — needs PartyCache.Id)
            var personDetails = PartyCacheFaker.GeneratePersonDetails(parties, faker);
            db.PartyPersonsCache.AddRange(personDetails);

            // AuthorizedUserCache (ValueGeneratedNever — manual Id)
            var authorizedUserFaker = new AuthorizedUserCacheFaker(activeHomeCenterNumbers);
            var authorizedUsers = authorizedUserFaker.Generate(10);
            db.AuthorizedUsersCache.AddRange(authorizedUsers);

            // OnLotHomeCache (UseIdentityAlwaysColumn — DB generates Id)
            var onLotHomeFaker = new OnLotHomeCacheFaker(activeHomeCenterNumbers);
            var onLotHomes = onLotHomeFaker.Generate(15);
            foreach (var h in onLotHomes) h.ClearDomainEvents();
            db.OnLotHomesCache.AddRange(onLotHomes);

            // LandParcelCache (UseIdentityAlwaysColumn — DB generates Id)
            var landParcelFaker = new LandParcelCacheFaker(activeHomeCenterNumbers);
            var landParcels = landParcelFaker.Generate(8);
            foreach (var l in landParcels) l.ClearDomainEvents();
            db.LandParcelsCache.AddRange(landParcels);

            await db.SaveChangesAsync(ct);
            logger.LogInformation(
                "Seeded cache: {Parties} parties, {Users} authorized users, {Homes} on-lot homes, {Land} land parcels.",
                parties.Count, authorizedUsers.Count, onLotHomes.Count, landParcels.Count);

            // ────────────────────────────────────────
            // Phase 2: Sequential FK chain
            // ────────────────────────────────────────

            var retailLocationIds = retailLocations.Select(r => r.Id).ToArray();
            var partyIds = parties.Select(p => p.Id).ToArray();

            // Sales (needs PartyId + RetailLocationId)
            var sales = SaleFaker.Generate(15, partyIds, retailLocationIds, faker);
            db.Sales.AddRange(sales);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} sales.", sales.Count);

            var saleIds = sales.Select(s => s.Id).ToArray();

            // Packages (needs SaleId)
            var packages = PackageFaker.Generate(saleIds, faker);
            db.Packages.AddRange(packages);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} packages.", packages.Count);

            // Delivery addresses (needs SaleId, 1:1) — created before package lines
            // so line fakers can snapshot real delivery context into details JSON.
            var deliveryAddresses = DeliveryAddressFaker.Generate(saleIds, faker, count: 10);

            // Build lookups so package line fakers receive actual entities (not just IDs).
            // This makes seed data joinable: home lines reference real OnLotHomeCache dimensions,
            // land lines use real LandParcelCache stock numbers, etc.
            var saleById = sales.ToDictionary(s => s.Id);
            var retailLocationById = retailLocations.ToDictionary(r => r.Id);
            var deliveryAddressBySaleId = deliveryAddresses.ToDictionary(da => da.SaleId);

            // Package lines (needs PackageId + actual cache entities for joinable data)
            var allLines = new List<PackageLine>();

            foreach (var package in packages)
            {
                var sale = saleById[package.SaleId];
                var retailLocation = retailLocationById[sale.RetailLocationId];
                deliveryAddressBySaleId.TryGetValue(package.SaleId, out var deliveryAddress);

                var onLotHome = onLotHomes.Count > 0 ? faker.PickRandom(onLotHomes) : null;
                var landParcel = landParcels.Count > 0 && faker.Random.Bool(0.7f)
                    ? faker.PickRandom(landParcels)
                    : null;
                var authorizedUser = authorizedUsers.Count > 0
                    ? faker.PickRandom(authorizedUsers)
                    : null;

                var lines = PackageLineFakers.GenerateForPackage(
                    package.Id, faker, onLotHome, landParcel, authorizedUser,
                    deliveryAddress, retailLocation);

                allLines.AddRange(lines);
            }

            db.PackageLines.AddRange(allLines);
            db.DeliveryAddresses.AddRange(deliveryAddresses);

            await db.SaveChangesAsync(ct);
            logger.LogInformation(
                "Seeded {Count} package lines, {Addresses} delivery addresses.",
                allLines.Count, deliveryAddresses.Count);

            // FundingRequestCache (ValueGeneratedNever — needs SaleId + PackageId)
            var fundingFaker = new FundingRequestCacheFaker();
            var salePkgPairs = packages
                .Where(p => p.IsPrimaryPackage)
                .Select(p => (p.SaleId, PackageId: p.Id))
                .ToArray();
            var fundingCount = Math.Min(8, salePkgPairs.Length);
            var fundingPairs = faker.PickRandom(salePkgPairs, fundingCount).ToArray();
            var fundingRequests = fundingFaker.Generate(fundingCount);

            for (var i = 0; i < fundingCount; i++)
            {
                fundingRequests[i].SaleId = fundingPairs[i].SaleId;
                fundingRequests[i].PackageId = fundingPairs[i].PackageId;
            }

            db.FundingRequestsCache.AddRange(fundingRequests);

            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} funding request cache entries.", fundingRequests.Count);
        }
    }
}
