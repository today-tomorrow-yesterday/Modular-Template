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

        // Deterministic seed — all Bogus output (names, addresses, dollar amounts, PickRandom
        // choices) will be identical across database recreations and developer machines.
        Bogus.Randomizer.Seed = new Random(SeedConstants.RandomSeed);
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

            // Only assign sales to active retail locations — HC 500 is inactive.
            var retailLocationIds = retailLocations.Where(r => r.IsActive).Select(r => r.Id).ToArray();
            var partyIds = parties.Select(p => p.Id).ToArray();

            // Sales (needs PartyId + RetailLocationId)
            var sales = SaleFaker.Generate(15, partyIds, retailLocationIds, faker);
            for (var i = 0; i < sales.Count; i++)
                SeedConstants.OverridePublicId(sales[i], SeedConstants.DeterministicGuid("sale", i));
            db.Sales.AddRange(sales);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} sales.", sales.Count);

            var saleIds = sales.Select(s => s.Id).ToArray();

            // Packages (needs SaleId)
            var packages = PackageFaker.Generate(saleIds, faker);
            for (var i = 0; i < packages.Count; i++)
                SeedConstants.OverridePublicId(packages[i], SeedConstants.DeterministicGuid("package", i));
            db.Packages.AddRange(packages);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Seeded {Count} packages.", packages.Count);

            // Delivery addresses (needs SaleId, 1:1) — created before package lines
            // so line fakers can snapshot real delivery context into details JSON.
            var deliveryAddresses = DeliveryAddressFaker.Generate(saleIds, faker, count: 10);
            for (var i = 0; i < deliveryAddresses.Count; i++)
                SeedConstants.OverridePublicId(deliveryAddresses[i], SeedConstants.DeterministicGuid("delivery-address", i));

            // Build lookups so package line fakers receive actual entities (not just IDs).
            // This makes seed data joinable: home lines reference real OnLotHomeCache dimensions,
            // land lines use real LandParcelCache stock numbers, etc.
            var saleById = sales.ToDictionary(s => s.Id);
            var retailLocationById = retailLocations.ToDictionary(r => r.Id);
            var deliveryAddressBySaleId = deliveryAddresses.ToDictionary(da => da.SaleId);

            // Group cache entities by home center so each package references inventory
            // from its own home center — matching production lookup behavior.
            var onLotHomesByHC = onLotHomes
                .GroupBy(h => h.RefHomeCenterNumber)
                .ToDictionary(g => g.Key, g => g.ToList());
            var landParcelsByHC = landParcels
                .GroupBy(l => l.RefHomeCenterNumber)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Package lines (needs PackageId + actual cache entities for joinable data).
            // Lines are added through the Package aggregate so RecalculateGrossProfit()
            // runs automatically — seeded packages get correct GP values.
            var lineCount = 0;

            foreach (var package in packages)
            {
                var sale = saleById[package.SaleId];
                var retailLocation = retailLocationById[sale.RetailLocationId];
                var hcNumber = retailLocation.RefHomeCenterNumber ?? 0;
                deliveryAddressBySaleId.TryGetValue(package.SaleId, out var deliveryAddress);

                // Pick cache entities from the SAME home center as the sale's retail location.
                // In production, handlers look up inventory by home center — this ensures the
                // FK + details data is consistent with what a real lookup would return.
                var homesForHC = onLotHomesByHC.GetValueOrDefault(hcNumber);
                var onLotHome = homesForHC is { Count: > 0 } ? faker.PickRandom(homesForHC) : null;

                var parcelsForHC = landParcelsByHC.GetValueOrDefault(hcNumber);
                var landParcel = parcelsForHC is { Count: > 0 } && faker.Random.Bool(0.7f)
                    ? faker.PickRandom(parcelsForHC)
                    : null;

                var usersForHC = authorizedUsers
                    .Where(u => u.AuthorizedHomeCenters?.Contains(hcNumber) == true)
                    .ToList();
                var authorizedUser = usersForHC.Count > 0 ? faker.PickRandom(usersForHC) : null;

                var lines = PackageLineFakers.GenerateForPackage(
                    package.Id, faker, onLotHome, landParcel, authorizedUser,
                    deliveryAddress, retailLocation);

                foreach (var line in lines)
                    package.AddLine(line);

                lineCount += lines.Count;
            }

            db.DeliveryAddresses.AddRange(deliveryAddresses);

            await db.SaveChangesAsync(ct);
            logger.LogInformation(
                "Seeded {Count} package lines, {Addresses} delivery addresses.",
                lineCount, deliveryAddresses.Count);

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
