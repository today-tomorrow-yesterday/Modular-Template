using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal static class PackageLineFakers
{
    public static List<PackageLine> GenerateForPackage(
        int packageId,
        Bogus.Faker faker,
        int? onLotHomeId = null,
        int? landParcelId = null,
        int? authorizedUserId = null)
    {
        var lines = new List<PackageLine>();

        // Home line — always present (1:1)
        lines.Add(CreateHomeLine(packageId, faker, onLotHomeId));

        // Land line — ~70% of packages
        if (faker.Random.Bool(0.7f))
            lines.Add(CreateLandLine(packageId, faker, landParcelId));

        // Tax line — ~80% of packages
        if (faker.Random.Bool(0.8f))
            lines.Add(CreateTaxLine(packageId, faker));

        // Insurance line — ~50%, sometimes 2
        if (faker.Random.Bool(0.5f))
        {
            lines.Add(CreateInsuranceLine(packageId, faker, sortOrder: 1));
            if (faker.Random.Bool(0.3f))
                lines.Add(CreateInsuranceLine(packageId, faker, sortOrder: 2));
        }

        // Warranty line — ~40% of packages
        if (faker.Random.Bool(0.4f))
            lines.Add(CreateWarrantyLine(packageId, faker));

        // Trade-in line — ~25%
        if (faker.Random.Bool(0.25f))
            lines.Add(CreateTradeInLine(packageId, faker, sortOrder: 1));

        // Sales team line — ~80% of packages
        if (faker.Random.Bool(0.8f))
            lines.Add(CreateSalesTeamLine(packageId, faker, authorizedUserId));

        // Project cost lines — ~60%, 1-3 items
        if (faker.Random.Bool(0.6f))
        {
            var count = faker.Random.Int(1, 3);
            for (var i = 0; i < count; i++)
                lines.Add(CreateProjectCostLine(packageId, faker, sortOrder: i + 1));
        }

        // Credit line — ~30%
        if (faker.Random.Bool(0.3f))
            lines.Add(CreateCreditLine(packageId, faker));

        return lines;
    }

    private static HomeLine CreateHomeLine(int packageId, Bogus.Faker faker, int? onLotHomeId)
    {
        var homeType = faker.PickRandom<HomeType>();
        var retailPrice = faker.Finance.Amount(45_000m, 180_000m);
        var cost = retailPrice * faker.Random.Decimal(0.60m, 0.80m);

        return HomeLine.Create(
            packageId: packageId,
            salePrice: retailPrice - faker.Finance.Amount(0m, 5_000m),
            estimatedCost: cost,
            retailSalePrice: retailPrice,
            responsibility: Responsibility.Buyer,
            details: HomeDetails.Create(
                homeType: homeType,
                homeSourceType: onLotHomeId.HasValue ? HomeSourceType.OnLot : HomeSourceType.Manual,
                stockNumber: onLotHomeId.HasValue ? faker.Random.Replace("STK####") : null,
                modularType: faker.PickRandom<ModularType>(),
                make: faker.PickRandom("Clayton", "Champion", "Skyline"),
                model: faker.PickRandom("Summit", "Freedom", "Eclipse"),
                modelYear: faker.Random.Int(2024, 2026),
                lengthInFeet: faker.Random.Decimal(56m, 80m).RoundTo(0),
                widthInFeet: faker.PickRandom(14m, 16m, 28m, 32m),
                bedrooms: faker.Random.Int(2, 4),
                bathrooms: faker.PickRandom(1m, 1.5m, 2m, 2.5m, 3m),
                baseCost: cost * 0.85m,
                optionsCost: cost * 0.10m,
                freightCost: cost * 0.05m,
                invoiceCost: cost,
                numberOfFloorSections: faker.Random.Int(1, 3),
                numberOfWheels: faker.Random.Int(4, 12),
                numberOfAxles: faker.Random.Int(2, 6),
                wheelAndAxlesOption: faker.PickRandom<WheelAndAxlesOption>(),
                distanceMiles: faker.Random.Double(10, 200)),
            onLotHomeId: onLotHomeId);
    }

    private static LandLine CreateLandLine(int packageId, Bogus.Faker faker, int? landParcelId)
    {
        var landCost = faker.Finance.Amount(15_000m, 60_000m);

        return LandLine.Create(
            packageId: packageId,
            salePrice: landCost + faker.Finance.Amount(1_000m, 8_000m),
            estimatedCost: landCost,
            retailSalePrice: landCost + faker.Finance.Amount(5_000m, 15_000m),
            responsibility: Responsibility.Buyer,
            details: LandDetails.Create(LandPurchaseType.CustomerHasLand),
            landParcelId: landParcelId);
    }

    private static TaxLine CreateTaxLine(int packageId, Bogus.Faker faker)
    {
        var taxAmount = faker.Finance.Amount(500m, 8_000m);

        return TaxLine.Create(
            packageId: packageId,
            salePrice: taxAmount,
            estimatedCost: 0m,
            retailSalePrice: taxAmount,
            shouldExcludeFromPricing: false,
            details: null);
    }

    private static InsuranceLine CreateInsuranceLine(int packageId, Bogus.Faker faker, int sortOrder)
    {
        var premium = faker.Finance.Amount(300m, 2_500m);

        return InsuranceLine.Create(
            packageId: packageId,
            salePrice: premium,
            estimatedCost: 0m,
            retailSalePrice: premium,
            responsibility: Responsibility.Buyer,
            shouldExcludeFromPricing: false,
            details: null,
            sortOrder: sortOrder);
    }

    private static WarrantyLine CreateWarrantyLine(int packageId, Bogus.Faker faker)
    {
        var warrantyAmount = faker.Finance.Amount(800m, 3_000m);

        return WarrantyLine.Create(
            packageId: packageId,
            salePrice: warrantyAmount,
            estimatedCost: warrantyAmount * 0.4m,
            retailSalePrice: warrantyAmount,
            shouldExcludeFromPricing: false,
            details: null);
    }

    private static TradeInLine CreateTradeInLine(int packageId, Bogus.Faker faker, int sortOrder)
    {
        var tradeAllowance = faker.Finance.Amount(5_000m, 25_000m);

        return TradeInLine.Create(
            packageId: packageId,
            salePrice: tradeAllowance,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Seller,
            details: null,
            sortOrder: sortOrder);
    }

    private static SalesTeamLine CreateSalesTeamLine(int packageId, Bogus.Faker faker, int? authorizedUserId)
    {
        // SalesTeamDetails has private setters — pass null (commission not yet calculated)
        return SalesTeamLine.Create(
            packageId: packageId,
            details: null);
    }

    private static ProjectCostLine CreateProjectCostLine(int packageId, Bogus.Faker faker, int sortOrder)
    {
        var cost = faker.Finance.Amount(200m, 5_000m);

        return ProjectCostLine.Create(
            packageId: packageId,
            salePrice: cost,
            estimatedCost: cost,
            retailSalePrice: cost,
            responsibility: Responsibility.Buyer,
            shouldExcludeFromPricing: false,
            details: ProjectCostDetails.Create(
                categoryId: faker.Random.Int(1, 5),
                itemId: faker.Random.Int(1, 20),
                itemDescription: faker.PickRandom("Wheels & Axles", "Setup", "Skirting", "Steps", "AC Unit")),
            sortOrder: sortOrder);
    }

    private static CreditLine CreateCreditLine(int packageId, Bogus.Faker faker)
    {
        return faker.Random.Bool()
            ? CreditLine.CreateDownPayment(packageId, faker.Finance.Amount(1_000m, 10_000m))
            : CreditLine.CreateConcession(packageId, faker.Finance.Amount(500m, 3_000m));
    }
}
