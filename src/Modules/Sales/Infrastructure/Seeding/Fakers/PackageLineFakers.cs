using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Credits;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.Packages.Land;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.Domain.Packages.SalesTeam;
using Modules.Sales.Domain.Packages.Tax;
using Modules.Sales.Domain.Packages.TradeIns;
using Modules.Sales.Domain.Packages.Warranty;
using Modules.Sales.Domain.RetailLocations;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal static class PackageLineFakers
{
    // Shared context so downstream lines (insurance, warranty, tax) can snapshot
    // the home and delivery state that was already generated for this package.
    private sealed record SeedContext(
        HomeDetails HomeDetails,
        string DeliveryState,
        string DeliveryCity,
        string DeliveryCounty,
        string DeliveryPostalCode,
        bool DeliveryIsWithinCityLimits,
        string OccupancyType,
        int HomeCenterNumber);

    public static List<PackageLine> GenerateForPackage(
        int packageId,
        Bogus.Faker faker,
        OnLotHomeCache? onLotHome = null,
        LandParcelCache? landParcel = null,
        AuthorizedUserCache? authorizedUser = null,
        DeliveryAddress? deliveryAddress = null,
        RetailLocation? retailLocation = null)
    {
        var lines = new List<PackageLine>();

        // Home line — always present (1:1). Generated first so downstream lines can snapshot it.
        var homeLine = CreateHomeLine(packageId, faker, onLotHome);
        lines.Add(homeLine);

        // Build seed context from actual entities when available, falling back to random data.
        // This ensures insurance/warranty/tax context snapshots match the real delivery address.
        var ctx = new SeedContext(
            HomeDetails: homeLine.Details!,
            DeliveryState: deliveryAddress?.State ?? faker.PickRandom("OH", "IN", "TX", "FL", "NC", "TN", "GA", "SC"),
            DeliveryCity: deliveryAddress?.City ?? faker.Address.City(),
            DeliveryCounty: deliveryAddress?.County ?? faker.Address.County(),
            DeliveryPostalCode: deliveryAddress?.PostalCode ?? faker.Address.ZipCode("#####"),
            DeliveryIsWithinCityLimits: deliveryAddress?.IsWithinCityLimits ?? faker.Random.Bool(0.6f),
            OccupancyType: deliveryAddress?.OccupancyType ?? faker.PickRandom("Primary", "Secondary"),
            HomeCenterNumber: retailLocation?.RefHomeCenterNumber ?? faker.Random.Int(1, 200));

        // Land line — ~70% of packages
        if (faker.Random.Bool(0.7f))
            lines.Add(CreateLandLine(packageId, faker, landParcel));

        // Tax line — ~80% of packages
        if (faker.Random.Bool(0.8f))
            lines.Add(CreateTaxLine(packageId, faker, ctx));

        // Insurance line — ~50%, sometimes 2 (HomeFirst + Outside)
        if (faker.Random.Bool(0.5f))
        {
            lines.Add(CreateInsuranceLine(packageId, faker, ctx, InsuranceType.HomeFirst, sortOrder: 1));
            if (faker.Random.Bool(0.3f))
                lines.Add(CreateInsuranceLine(packageId, faker, ctx, InsuranceType.Outside, sortOrder: 2));
        }

        // Warranty line — ~40% of packages
        if (faker.Random.Bool(0.4f))
            lines.Add(CreateWarrantyLine(packageId, faker, ctx));

        // Trade-in line — ~25%
        if (faker.Random.Bool(0.25f))
            lines.Add(CreateTradeInLine(packageId, faker, sortOrder: 1));

        // Sales team line — ~80% of packages
        if (faker.Random.Bool(0.8f))
            lines.Add(CreateSalesTeamLine(packageId, faker, authorizedUser));

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

    // --- Home line ---
    // When an OnLotHomeCache is provided, all dimensions, pricing, and identifiers are derived
    // from the cache entity so the package line data is joinable with the inventory cache.

    private static HomeLine CreateHomeLine(int packageId, Bogus.Faker faker, OnLotHomeCache? onLotHome)
    {
        var hasCache = onLotHome is not null;

        var homeType = hasCache
            ? MapConditionToHomeType(onLotHome!.Condition)
            : faker.PickRandom<HomeType>();

        var width = onLotHome?.Width ?? faker.PickRandom(14m, 16m, 28m, 32m);
        var length = onLotHome?.Length ?? faker.Random.Decimal(56m, 80m).RoundTo(0);
        var modelYear = onLotHome?.ModelYear ?? faker.Random.Int(2024, 2026);
        var make = onLotHome?.Make ?? faker.PickRandom("Clayton", "Champion", "Skyline");
        var model = onLotHome?.Model ?? faker.PickRandom("Summit", "Freedom", "Eclipse", "Pinnacle");
        var bedrooms = onLotHome?.NumberOfBedrooms ?? faker.Random.Int(2, 4);
        var bathrooms = hasCache && onLotHome!.NumberOfBathrooms.HasValue
            ? (decimal)onLotHome.NumberOfBathrooms.Value
            : faker.PickRandom(1m, 1.5m, 2m, 2.5m, 3m);
        var buildType = onLotHome?.BuildType ?? faker.PickRandom("Standard", "Custom", "Special Order");
        var serialNumber = onLotHome?.SerialNumber ?? faker.Random.Replace("CLT######AB");

        var retailPrice = onLotHome?.CurrentRetailPrice
            ?? onLotHome?.OriginalRetailPrice
            ?? faker.Finance.Amount(45_000m, 180_000m);
        var invoiceCost = onLotHome?.TotalInvoiceAmount
            ?? retailPrice * faker.Random.Decimal(0.60m, 0.80m);

        var baseCost = invoiceCost * 0.85m;
        var optionsCost = invoiceCost * 0.10m;
        var freightCost = invoiceCost * 0.05m;
        var taxOnInvoice = Math.Round(invoiceCost * 0.02m, 2);
        var rebate = Math.Round(invoiceCost * faker.Random.Decimal(0.01m, 0.03m), 2);
        var netInvoice = invoiceCost - rebate;
        var grossCost = netInvoice + taxOnInvoice;

        return HomeLine.Create(
            packageId: packageId,
            salePrice: retailPrice - faker.Finance.Amount(0m, 5_000m),
            estimatedCost: invoiceCost,
            retailSalePrice: retailPrice,
            responsibility: Responsibility.Buyer,
            details: HomeDetails.Create(
                homeType: homeType,
                homeSourceType: hasCache ? HomeSourceType.OnLot : HomeSourceType.Manual,
                stockNumber: hasCache ? onLotHome!.RefStockNumber : null,
                modularType: faker.PickRandom<ModularType>(),
                vendor: faker.PickRandom("CMH Manufacturing", "Cavco Industries", "Skyline Champion"),
                make: make,
                model: model,
                modelYear: modelYear,
                lengthInFeet: length,
                widthInFeet: width,
                bedrooms: bedrooms,
                bathrooms: bathrooms,
                squareFootage: $"{(int)(length * width)}",
                serialNumbers: [serialNumber],
                baseCost: baseCost,
                optionsCost: optionsCost,
                freightCost: freightCost,
                invoiceCost: invoiceCost,
                netInvoice: netInvoice,
                grossCost: grossCost,
                taxIncludedOnInvoice: taxOnInvoice,
                numberOfWheels: faker.Random.Int(4, 12),
                numberOfAxles: faker.Random.Int(2, 6),
                wheelAndAxlesOption: faker.PickRandom<WheelAndAxlesOption>(),
                numberOfFloorSections: faker.Random.Int(1, 3),
                carrierFrameDeposit: Math.Round(faker.Finance.Amount(200m, 800m), 2),
                rebateOnMfgInvoice: rebate,
                claytonBuilt: faker.Random.Bool(0.4f),
                buildType: buildType,
                inventoryReferenceId: hasCache ? Guid.NewGuid().ToString() : null,
                stateAssociationAndMhiDues: Math.Round(faker.Finance.Amount(50m, 300m), 2),
                partnerAssistance: Math.Round(faker.Finance.Amount(0m, 500m), 2),
                distanceMiles: faker.Random.Double(10, 200),
                propertyType: faker.PickRandom("SingleWide", "DoubleWide", "TripleWide"),
                purchaseOption: faker.PickRandom("Cash", "Finance", "Lease"),
                listingPrice: retailPrice + faker.Finance.Amount(0m, 3_000m),
                accountNumber: faker.Random.Replace("ACC-######"),
                displayAccountId: faker.Random.Replace("DA-####"),
                streetAddress: faker.Address.StreetAddress(),
                city: faker.Address.City(),
                state: faker.Address.State(),
                zipCode: faker.Address.ZipCode("#####")),
            onLotHomeId: hasCache ? onLotHome!.Id : null);
    }

    private static HomeType MapConditionToHomeType(HomeCondition? condition) => condition switch
    {
        HomeCondition.New => HomeType.New,
        HomeCondition.Used => HomeType.Used,
        HomeCondition.Repo => HomeType.Repo,
        _ => HomeType.New
    };

    // --- Land line ---
    // When a LandParcelCache is provided, forces HomeCenterOwnedLand scenario so the stock number,
    // cost, and sale price are joinable with the cache entity.

    private static LandLine CreateLandLine(int packageId, Bogus.Faker faker, LandParcelCache? landParcel)
    {
        var hasCache = landParcel is not null;
        var landCost = landParcel?.LandCost ?? faker.Finance.Amount(15_000m, 60_000m);
        var landSalePrice = landCost + faker.Finance.Amount(1_000m, 8_000m);

        // When a land parcel cache is provided, force HomeCenterOwnedLand scenario
        // so the stock number, cost, and pricing are joinable with the cache.
        var (purchaseType, customerLandType, landInclusion, typeOfLandWanted) = hasCache
            ? (LandPurchaseType.CustomerWantsToPurchaseLand,
               (CustomerLandType?)null,
               (LandInclusion?)null,
               (TypeOfLandWanted?)TypeOfLandWanted.HomeCenterOwnedLand)
            : faker.Random.Int(1, 4) switch
            {
                1 => (LandPurchaseType.CustomerHasLand,
                      (CustomerLandType?)CustomerLandType.CustomerOwnedLand,
                      (LandInclusion?)LandInclusion.CustomerLandPayoff,
                      (TypeOfLandWanted?)null),
                2 => (LandPurchaseType.CustomerWantsToPurchaseLand,
                      (CustomerLandType?)null,
                      (LandInclusion?)null,
                      (TypeOfLandWanted?)TypeOfLandWanted.LandPurchase),
                3 => (LandPurchaseType.CustomerWantsToPurchaseLand,
                      (CustomerLandType?)null,
                      (LandInclusion?)null,
                      (TypeOfLandWanted?)TypeOfLandWanted.HomeCenterOwnedLand),
                _ => (LandPurchaseType.CustomerHasLand,
                      (CustomerLandType?)CustomerLandType.PrivateProperty,
                      (LandInclusion?)LandInclusion.HomeOnly,
                      (TypeOfLandWanted?)null)
            };

        return LandLine.Create(
            packageId: packageId,
            salePrice: landSalePrice,
            estimatedCost: landCost,
            retailSalePrice: landSalePrice + faker.Finance.Amount(2_000m, 10_000m),
            responsibility: Responsibility.Buyer,
            details: LandDetails.Create(
                landPurchaseType: purchaseType,
                customerLandType: customerLandType,
                landInclusion: landInclusion,
                typeOfLandWanted: typeOfLandWanted,
                estimatedValue: landCost + faker.Finance.Amount(5_000m, 20_000m),
                sizeInAcres: Math.Round(faker.Random.Decimal(0.25m, 10m), 2),
                propertyOwner: faker.Name.FullName(),
                financedBy: faker.PickRandom("Bank of America", "Wells Fargo", "Local Credit Union", "Owner Financed"),
                payoffAmountFinancing: landInclusion == LandInclusion.CustomerLandPayoff
                    ? Math.Round(landCost * faker.Random.Decimal(0.5m, 0.9m), 2)
                    : null,
                landEquity: Math.Round(faker.Finance.Amount(5_000m, 30_000m), 2),
                originalPurchaseDate: new DateTimeOffset(
                    faker.Date.Past(10, DateTime.UtcNow.AddYears(-1)), TimeSpan.Zero),
                originalPurchasePrice: landCost - faker.Finance.Amount(2_000m, 10_000m),
                propertyOwnerPhoneNumber: faker.Phone.PhoneNumber("##########"),
                propertyLotRent: customerLandType == CustomerLandType.CommunityOrNeighborhood
                    ? Math.Round(faker.Finance.Amount(200m, 800m), 2)
                    : null,
                realtor: typeOfLandWanted == TypeOfLandWanted.LandPurchase
                    ? faker.Name.FullName()
                    : null,
                purchasePrice: typeOfLandWanted == TypeOfLandWanted.LandPurchase
                    ? landSalePrice
                    : null,
                landStockNumber: hasCache
                    ? landParcel!.RefStockNumber
                    : typeOfLandWanted == TypeOfLandWanted.HomeCenterOwnedLand
                        ? faker.Random.Replace("LND####")
                        : null,
                landCost: typeOfLandWanted == TypeOfLandWanted.HomeCenterOwnedLand
                    ? landCost
                    : null,
                landSalesPrice: typeOfLandWanted == TypeOfLandWanted.HomeCenterOwnedLand
                    ? landSalePrice
                    : null,
                communityNumber: faker.Random.Int(100, 999),
                communityName: $"{faker.Address.City()} Estates",
                communityManagerName: faker.Name.FullName(),
                communityManagerPhoneNumber: faker.Phone.PhoneNumber("##########"),
                communityManagerEmail: faker.Internet.Email(),
                communityMonthlyCost: Math.Round(faker.Finance.Amount(300m, 1_200m), 2)),
            landParcelId: hasCache ? landParcel!.Id : null);
    }

    private static TaxLine CreateTaxLine(int packageId, Bogus.Faker faker, SeedContext ctx)
    {
        var taxAmount = faker.Finance.Amount(500m, 8_000m);

        return TaxLine.Create(
            packageId: packageId,
            salePrice: taxAmount,
            estimatedCost: 0m,
            retailSalePrice: taxAmount,
            shouldExcludeFromPricing: false,
            details: TaxDetails.Create(
                previouslyTitled: faker.PickRandom("Y", "N"),
                taxExemptionId: faker.Random.Bool(0.1f) ? faker.Random.Int(1, 5) : null,
                questionAnswers: [],
                taxes: [],
                errors: null,
                taxExemptionDescription: faker.Random.Bool(0.1f) ? "Veteran Exemption" : null,
                stateCode: ctx.DeliveryState,
                deliveryCity: ctx.DeliveryCity,
                deliveryCounty: ctx.DeliveryCounty,
                deliveryPostalCode: ctx.DeliveryPostalCode,
                deliveryIsWithinCityLimits: ctx.DeliveryIsWithinCityLimits));
    }

    private static InsuranceLine CreateInsuranceLine(
        int packageId, Bogus.Faker faker, SeedContext ctx,
        InsuranceType insuranceType, int sortOrder)
    {
        var premium = faker.Finance.Amount(300m, 2_500m);
        var coverage = premium * faker.Random.Decimal(2m, 5m);
        var home = ctx.HomeDetails;

        var details = insuranceType == InsuranceType.HomeFirst
            ? InsuranceDetails.Create(
                insuranceType: InsuranceType.HomeFirst,
                coverageAmount: coverage,
                hasFoundationOrMasonry: faker.Random.Bool(0.2f),
                inParkOrSubdivision: faker.Random.Bool(0.3f),
                isLandOwnedByCustomer: faker.Random.Bool(0.5f),
                isPremiumFinanced: faker.Random.Bool(0.7f),
                quoteId: null,
                companyName: "HomeFirst Insurance Co",
                maxCoverage: coverage * 2m,
                totalPremium: premium,
                providerName: null,
                tempLinkId: faker.Random.Int(1, 999),
                quotedAt: DateTimeOffset.UtcNow.AddDays(-faker.Random.Int(1, 30)),
                homeStockNumber: home.StockNumber,
                homeModelYear: home.ModelYear,
                homeLengthInFeet: home.LengthInFeet,
                homeWidthInFeet: home.WidthInFeet,
                homeCondition: home.HomeType.ToString(),
                deliveryState: ctx.DeliveryState,
                deliveryPostalCode: ctx.DeliveryPostalCode,
                deliveryCity: ctx.DeliveryCity,
                deliveryIsWithinCityLimits: ctx.DeliveryIsWithinCityLimits,
                occupancyType: ctx.OccupancyType)
            : InsuranceDetails.Create(
                insuranceType: InsuranceType.Outside,
                coverageAmount: coverage,
                hasFoundationOrMasonry: false,
                inParkOrSubdivision: false,
                isLandOwnedByCustomer: false,
                isPremiumFinanced: false,
                providerName: faker.PickRandom("State Farm", "Allstate", "Foremost", "American Modern"),
                totalPremium: premium);

        return InsuranceLine.Create(
            packageId: packageId,
            salePrice: premium,
            estimatedCost: 0m,
            retailSalePrice: premium,
            responsibility: Responsibility.Buyer,
            shouldExcludeFromPricing: false,
            details: details,
            sortOrder: sortOrder);
    }

    private static WarrantyLine CreateWarrantyLine(int packageId, Bogus.Faker faker, SeedContext ctx)
    {
        var warrantyAmount = faker.Finance.Amount(800m, 3_000m);
        var home = ctx.HomeDetails;

        return WarrantyLine.Create(
            packageId: packageId,
            salePrice: warrantyAmount,
            estimatedCost: warrantyAmount * 0.4m,
            retailSalePrice: warrantyAmount,
            shouldExcludeFromPricing: false,
            details: WarrantyDetails.Create(
                warrantyAmount: warrantyAmount,
                salesTaxPremium: Math.Round(warrantyAmount * 0.08m, 2),
                warrantySelected: true,
                quotedAt: DateTimeOffset.UtcNow.AddDays(-faker.Random.Int(1, 30)),
                homeModelYear: home.ModelYear,
                homeModularType: home.ModularType?.ToString(),
                homeWidthInFeet: home.WidthInFeet,
                homeCondition: home.HomeType.ToString(),
                deliveryState: ctx.DeliveryState,
                deliveryPostalCode: ctx.DeliveryPostalCode,
                deliveryIsWithinCityLimits: ctx.DeliveryIsWithinCityLimits,
                homeCenterNumber: ctx.HomeCenterNumber));
    }

    private static TradeInLine CreateTradeInLine(int packageId, Bogus.Faker faker, int sortOrder)
    {
        var tradeAllowance = faker.Finance.Amount(5_000m, 25_000m);
        var isHome = faker.Random.Bool(0.6f);

        return TradeInLine.Create(
            packageId: packageId,
            salePrice: tradeAllowance,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Seller,
            details: TradeInDetails.Create(
                tradeType: isHome ? "Home" : "Vehicle",
                year: faker.Random.Int(2010, 2024),
                make: isHome
                    ? faker.PickRandom("Clayton", "Champion", "Skyline")
                    : faker.PickRandom("Ford", "Chevrolet", "Toyota"),
                model: isHome
                    ? faker.PickRandom("Summit", "Freedom", "Eclipse")
                    : faker.PickRandom("F-150", "Silverado", "Tacoma"),
                tradeAllowance: tradeAllowance,
                payoffAmount: Math.Round(tradeAllowance * faker.Random.Decimal(0.2m, 0.8m), 2),
                bookInAmount: Math.Round(tradeAllowance * faker.Random.Decimal(0.5m, 0.9m), 2),
                floorWidth: isHome ? faker.PickRandom(14m, 16m, 28m) : null,
                floorLength: isHome ? faker.Random.Decimal(56m, 76m).RoundTo(0) : null),
            sortOrder: sortOrder);
    }

    // --- Sales team line ---
    // When an AuthorizedUserCache is provided, the primary member's name and employee number
    // are derived from the cache so the data is joinable with the authorized users table.

    private static SalesTeamLine CreateSalesTeamLine(
        int packageId, Bogus.Faker faker, AuthorizedUserCache? authorizedUser)
    {
        var hasCache = authorizedUser is not null;
        var primaryName = hasCache ? authorizedUser!.DisplayName : faker.Name.FullName();
        var primaryEmployeeNumber = hasCache ? authorizedUser!.EmployeeNumber : faker.Random.Int(10000, 99999);
        var primaryUserId = hasCache ? authorizedUser!.Id : (int?)null;

        var members = new List<SalesTeamMember>
        {
            SalesTeamMember.Create(
                authorizedUserId: primaryUserId,
                role: SalesTeamRole.Primary,
                commissionSplitPercentage: 100m,
                employeeName: primaryName,
                employeeNumber: primaryEmployeeNumber)
        };

        if (faker.Random.Bool(0.3f))
        {
            members[0] = SalesTeamMember.Create(
                authorizedUserId: primaryUserId,
                role: SalesTeamRole.Primary,
                commissionSplitPercentage: 50m,
                employeeName: primaryName,
                employeeNumber: primaryEmployeeNumber);

            members.Add(SalesTeamMember.Create(
                authorizedUserId: null,
                role: SalesTeamRole.Secondary,
                commissionSplitPercentage: 50m,
                employeeName: faker.Name.FullName(),
                employeeNumber: faker.Random.Int(10000, 99999)));
        }

        return SalesTeamLine.Create(
            packageId: packageId,
            details: SalesTeamDetails.Create(members));
    }

    // Well-known user-managed project costs (excludes auto-generated like W&A, Use Tax, etc.)
    private static readonly (int CatId, int ItemId, string CatDesc, string ItemDesc)[] KnownProjectCosts =
    [
        (4, 1, "Setup & Delivery", "Setup"),
        (4, 2, "Setup & Delivery", "Delivery"),
        (5, 1, "Site Preparation", "Skirting"),
        (5, 2, "Site Preparation", "Steps & Railings"),
        (5, 3, "Site Preparation", "AC Unit"),
        (6, 1, "Utilities", "Electric Hookup"),
        (6, 2, "Utilities", "Plumbing Hookup"),
        (7, 1, "Permits & Fees", "Building Permit"),
        (7, 2, "Permits & Fees", "Impact Fee"),
        (8, 1, "Miscellaneous", "Furniture Package"),
        (8, 2, "Miscellaneous", "Appliance Upgrade"),
        (ProjectCostCategories.Refurbishment, ProjectCostItems.Cleaning, "Refurbishment", "Cleaning"),
        (ProjectCostCategories.Refurbishment, ProjectCostItems.RepairRefurb, "Refurbishment", "Repair / Refurb"),
        (ProjectCostCategories.Decorating, ProjectCostItems.DecoratingDrapes, "Decorating", "Drapes"),
    ];

    private static ProjectCostLine CreateProjectCostLine(int packageId, Bogus.Faker faker, int sortOrder)
    {
        var cost = faker.Finance.Amount(200m, 5_000m);
        var pick = faker.PickRandom(KnownProjectCosts);
        var profitPct = faker.Random.Decimal(0m, 25m).RoundTo(2);

        return ProjectCostLine.Create(
            packageId: packageId,
            salePrice: cost,
            estimatedCost: Math.Round(cost * faker.Random.Decimal(0.6m, 0.95m), 2),
            retailSalePrice: cost,
            responsibility: Responsibility.Buyer,
            shouldExcludeFromPricing: false,
            details: ProjectCostDetails.Create(
                categoryId: pick.CatId,
                itemId: pick.ItemId,
                itemDescription: pick.ItemDesc,
                categoryDescription: pick.CatDesc,
                categoryIsCreditConsideration: faker.Random.Bool(0.2f),
                categoryIsLandDot: faker.Random.Bool(0.1f),
                categoryRestrictFha: faker.Random.Bool(0.15f),
                categoryRestrictCss: faker.Random.Bool(0.1f),
                categoryDisplayForCash: faker.Random.Bool(0.8f),
                itemStatus: "Active",
                itemIsFeeItem: faker.Random.Bool(0.2f),
                itemIsCssRestricted: faker.Random.Bool(0.1f),
                itemIsFhaRestricted: faker.Random.Bool(0.15f),
                itemIsDisplayForCash: faker.Random.Bool(0.8f),
                itemIsRestrictOptionPrice: faker.Random.Bool(0.1f),
                itemIsRestrictCssCost: faker.Random.Bool(0.1f),
                itemIsHopeRefundsIncluded: faker.Random.Bool(0.05f),
                itemProfitPercentage: profitPct),
            sortOrder: sortOrder);
    }

    private static CreditLine CreateCreditLine(int packageId, Bogus.Faker faker)
    {
        return faker.Random.Bool()
            ? CreditLine.CreateDownPayment(packageId, faker.Finance.Amount(1_000m, 10_000m))
            : CreditLine.CreateConcession(packageId, faker.Finance.Amount(500m, 3_000m));
    }
}
