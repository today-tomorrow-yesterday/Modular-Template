using Modules.Sales.Domain.FundingCache;
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
using System.Text.Json;

namespace Modules.Sales.Application.Packages.GetPackageById;

internal static class PackageDetailMapper
{
    public static PackageDetailResponse MapToResponse(
        Package package,
        FundingRequestCache? funding = null,
        IReadOnlyDictionary<int, int>? employeeNumberMap = null)
    {
        var home = package.Lines.OfType<HomeLine>().SingleOrDefault();
        var land = package.Lines.OfType<LandLine>().SingleOrDefault();
        var tax = package.Lines.OfType<TaxLine>().SingleOrDefault();
        var insurance = package.Lines.OfType<InsuranceLine>().FirstOrDefault();
        var warranty = package.Lines.OfType<WarrantyLine>().SingleOrDefault();
        var salesTeam = package.Lines.OfType<SalesTeamLine>().SingleOrDefault();

        var tradeInLines = package.Lines.OfType<TradeInLine>().OrderBy(l => l.SortOrder);
        var projectCostLines = package.Lines.OfType<ProjectCostLine>().OrderBy(l => l.SortOrder);
        var creditLines = package.Lines.OfType<CreditLine>();

        var downPayment = creditLines.SingleOrDefault(l => l.IsDownPayment);
        var concession = creditLines.SingleOrDefault(l => l.IsConcession);

        return new PackageDetailResponse(
            Id: package.PublicId,
            Name: package.Name,
            Ranking: package.Ranking,
            IsPrimaryPackage: package.IsPrimaryPackage,
            Status: package.Status.ToString(),
            GrossProfit: package.GrossProfit,
            CommissionableGrossProfit: package.CommissionableGrossProfit,
            MustRecalculateTaxes: package.MustRecalculateTaxes,
            FundingRequestIds: funding is not null ? [funding.RefFundingRequestId] : [],
            Home: MapHome(home),
            Land: MapLand(land),
            Tax: MapTax(tax),
            Insurance: MapInsurance(insurance),
            Warranty: MapWarranty(warranty),
            DownPayment: MapDownPayment(downPayment),
            Concessions: MapConcessions(concession),
            TradeIns: tradeInLines.Select(MapTradeIn).ToArray(),
            SalesTeam: MapSalesTeam(salesTeam, employeeNumberMap),
            ProjectCosts: projectCostLines.Select((line, index) => MapProjectCost(line, index)).ToArray(),
            Funding: MapFunding(funding));
    }

    private static HomeSectionResponse? MapHome(HomeLine? line)
    {
        if (line is null) return null;
        var d = line.Details;

        return new HomeSectionResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing,
            IsProductRemovedFromInventory: line.IsProductRemovedFromInventory,
            HomeType: d?.HomeType.ToString(),
            HomeSourceType: d?.HomeSourceType.ToString(),
            StockNumber: d?.StockNumber,
            Make: d?.Make,
            Model: d?.Model,
            ModelYear: d?.ModelYear,
            LengthInFeet: d?.LengthInFeet,
            WidthInFeet: d?.WidthInFeet,
            Bedrooms: d?.Bedrooms,
            Bathrooms: d?.Bathrooms,
            SquareFootage: d?.SquareFootage,
            NumberOfFloorSections: d?.NumberOfFloorSections,
            ModularType: d?.ModularType?.ToString(),
            BuildType: d?.BuildType,
            ClaytonBuilt: d?.ClaytonBuilt,
            Vendor: d?.Vendor,
            BaseCost: d?.BaseCost,
            OptionsCost: d?.OptionsCost,
            FreightCost: d?.FreightCost,
            InvoiceCost: d?.InvoiceCost,
            NetInvoice: d?.NetInvoice,
            GrossCost: d?.GrossCost,
            TaxIncludedOnInvoice: d?.TaxIncludedOnInvoice,
            RebateOnMfgInvoice: d?.RebateOnMfgInvoice,
            StateAssociationAndMhiDues: d?.StateAssociationAndMhiDues,
            PartnerAssistance: d?.PartnerAssistance,
            NumberOfWheels: d?.NumberOfWheels,
            NumberOfAxles: d?.NumberOfAxles,
            WheelAndAxlesOption: d?.WheelAndAxlesOption?.ToString(),
            CarrierFrameDeposit: d?.CarrierFrameDeposit,
            DistanceMiles: d?.DistanceMiles,
            PropertyType: d?.PropertyType,
            PurchaseOption: d?.PurchaseOption,
            ListingPrice: d?.ListingPrice,
            AccountNumber: d?.AccountNumber,
            DisplayAccountId: d?.DisplayAccountId,
            StreetAddress: d?.StreetAddress,
            City: d?.City,
            State: d?.State,
            ZipCode: d?.ZipCode,
            InventoryReferenceId: d?.InventoryReferenceId,
            SerialNumbers: d?.SerialNumbers);
    }

    private static LandSectionResponse? MapLand(LandLine? line)
    {
        if (line is null) return null;
        var d = line.Details;
        if (d is null) return new LandSectionResponse(
            line.SalePrice, line.EstimatedCost, line.RetailSalePrice,
            line.Responsibility?.ToString(), line.ShouldExcludeFromPricing,
            line.IsProductRemovedFromInventory,
            string.Empty, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null);

        return new LandSectionResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing,
            IsProductRemovedFromInventory: line.IsProductRemovedFromInventory,
            LandPurchaseType: d.LandPurchaseType.ToString(),
            CustomerLandType: d.CustomerLandType?.ToString(),
            LandInclusion: d.LandInclusion?.ToString(),
            TypeOfLandWanted: d.TypeOfLandWanted?.ToString(),
            EstimatedValue: d.EstimatedValue,
            SizeInAcres: d.SizeInAcres,
            PropertyOwner: d.PropertyOwner,
            FinancedBy: d.FinancedBy,
            PayoffAmountFinancing: d.PayoffAmountFinancing,
            LandEquity: d.LandEquity,
            OriginalPurchaseDate: d.OriginalPurchaseDate,
            OriginalPurchasePrice: d.OriginalPurchasePrice,
            PropertyOwnerPhoneNumber: d.PropertyOwnerPhoneNumber,
            PropertyLotRent: d.PropertyLotRent,
            Realtor: d.Realtor,
            PurchasePrice: d.PurchasePrice,
            LandStockNumber: d.LandStockNumber,
            LandCost: d.LandCost,
            LandSalesPrice: d.LandSalesPrice,
            CommunityNumber: d.CommunityNumber,
            CommunityName: d.CommunityName,
            CommunityManagerName: d.CommunityManagerName,
            CommunityManagerPhoneNumber: d.CommunityManagerPhoneNumber,
            CommunityManagerEmail: d.CommunityManagerEmail,
            CommunityMonthlyCost: d.CommunityMonthlyCost);
    }

    private static TaxSectionResponse? MapTax(TaxLine? line)
    {
        if (line is null) return null;
        var d = line.Details;

        return new TaxSectionResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing,
            PreviouslyTitled: d?.PreviouslyTitled,
            TaxExemptionId: d?.TaxExemptionId,
            StateTaxQuestionAnswers: d?.StateTaxQuestionAnswers?
                .Select(qa => new TaxQuestionAnswerResponse(qa.QuestionNumber, qa.QuestionText, ParseBoolAnswer(qa.Answer)))
                .ToArray() ?? [],
            TaxItems: d?.Taxes?
                .Select(t => new PackageTaxItemResponse(t.Name, t.IsOverridden, t.CalculatedAmount, t.ChargedAmount))
                .ToArray() ?? [],
            Errors: d?.Errors?.ToArray(),
            TaxExemptionDescription: d?.TaxExemptionDescription,
            StateCode: d?.StateCode,
            DeliveryCity: d?.DeliveryCity,
            DeliveryCounty: d?.DeliveryCounty,
            DeliveryPostalCode: d?.DeliveryPostalCode,
            DeliveryIsWithinCityLimits: d?.DeliveryIsWithinCityLimits);
    }

    private static InsuranceSectionResponse? MapInsurance(InsuranceLine? line)
    {
        if (line is null) return null;
        var d = line.Details;

        return new InsuranceSectionResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing,
            InsuranceType: d?.InsuranceType.ToString() ?? string.Empty,
            CoverageAmount: d?.CoverageAmount ?? 0m,
            HasFoundationOrMasonry: d?.HasFoundationOrMasonry ?? false,
            InParkOrSubdivision: d?.InParkOrSubdivision ?? false,
            IsLandOwnedByCustomer: d?.IsLandOwnedByCustomer ?? false,
            IsPremiumFinanced: d?.IsPremiumFinanced ?? false,
            QuoteId: d?.QuoteId,
            CompanyName: d?.CompanyName,
            MaxCoverage: d?.MaxCoverage,
            TotalPremium: d?.TotalPremium,
            ProviderName: d?.ProviderName,
            TempLinkId: d?.TempLinkId,
            QuotedAt: d?.QuotedAt,
            HomeStockNumber: d?.HomeStockNumber,
            HomeModelYear: d?.HomeModelYear,
            HomeLengthInFeet: d?.HomeLengthInFeet,
            HomeWidthInFeet: d?.HomeWidthInFeet,
            HomeCondition: d?.HomeCondition,
            DeliveryState: d?.DeliveryState,
            DeliveryPostalCode: d?.DeliveryPostalCode,
            DeliveryCity: d?.DeliveryCity,
            DeliveryIsWithinCityLimits: d?.DeliveryIsWithinCityLimits,
            OccupancyType: d?.OccupancyType);
    }

    private static WarrantySectionResponse? MapWarranty(WarrantyLine? line)
    {
        if (line is null) return null;
        var d = line.Details;

        return new WarrantySectionResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing,
            WarrantySelected: d?.WarrantySelected ?? false,
            WarrantyAmount: d?.WarrantyAmount,
            SalesTaxPremium: d?.SalesTaxPremium,
            QuotedAt: d?.QuotedAt,
            HomeModelYear: d?.HomeModelYear,
            HomeModularType: d?.HomeModularType,
            HomeWidthInFeet: d?.HomeWidthInFeet,
            HomeCondition: d?.HomeCondition,
            DeliveryState: d?.DeliveryState,
            DeliveryPostalCode: d?.DeliveryPostalCode,
            DeliveryIsWithinCityLimits: d?.DeliveryIsWithinCityLimits,
            HomeCenterNumber: d?.HomeCenterNumber);
    }

    private static DownPaymentResponse? MapDownPayment(CreditLine? line)
    {
        if (line is null) return null;

        return new DownPaymentResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing);
    }

    private static ConcessionsResponse? MapConcessions(CreditLine? line)
    {
        if (line is null) return null;

        return new ConcessionsResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing);
    }

    private static TradeInResponse MapTradeIn(TradeInLine line)
    {
        var d = line.Details;

        return new TradeInResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing,
            TradeType: d?.TradeType ?? string.Empty,
            Year: d?.Year ?? 0,
            Make: d?.Make ?? string.Empty,
            Model: d?.Model ?? string.Empty,
            FloorWidth: d?.FloorWidth,
            FloorLength: d?.FloorLength,
            TradeAllowance: d?.TradeAllowance ?? 0m,
            PayoffAmount: d?.PayoffAmount ?? 0m,
            BookInAmount: d?.BookInAmount ?? 0m);
    }

    private static SalesTeamMemberResponse[] MapSalesTeam(
        SalesTeamLine? line, IReadOnlyDictionary<int, int>? employeeNumberMap)
    {
        if (line?.Details?.SalesTeamMembers is not { Count: > 0 } members)
            return [];

        return members
            .Select((m, index) => new SalesTeamMemberResponse(
                Role: FormatRole(m.Role),
                AuthorizedUserId: m.AuthorizedUserId,
                EmployeeNumber: ResolveEmployeeNumber(m.AuthorizedUserId, employeeNumberMap),
                EmployeeName: m.EmployeeName,
                SortOrder: index,
                SalePrice: line.SalePrice,
                EstimatedCost: line.EstimatedCost,
                RetailSalePrice: line.RetailSalePrice,
                Responsibility: line.Responsibility?.ToString(),
                ShouldExcludeFromPricing: line.ShouldExcludeFromPricing,
                CommissionSplitPercentage: m.CommissionSplitPercentage,
                CommissionAmount: m.CommissionAmount))
            .ToArray();
    }

    private static ProjectCostResponse MapProjectCost(ProjectCostLine line, int index)
    {
        var d = line.Details;

        return new ProjectCostResponse(
            SalePrice: line.SalePrice,
            EstimatedCost: line.EstimatedCost,
            RetailSalePrice: line.RetailSalePrice,
            Responsibility: line.Responsibility?.ToString(),
            ShouldExcludeFromPricing: line.ShouldExcludeFromPricing,
            CategoryNumber: d?.CategoryId ?? 0,
            ItemId: d?.ItemId ?? 0,
            ItemDescription: d?.ItemDescription,
            SortOrder: index,
            CategoryDescription: d?.CategoryDescription,
            CategoryIsCreditConsideration: d?.CategoryIsCreditConsideration,
            CategoryIsLandDot: d?.CategoryIsLandDot,
            CategoryRestrictFha: d?.CategoryRestrictFha,
            CategoryRestrictCss: d?.CategoryRestrictCss,
            CategoryDisplayForCash: d?.CategoryDisplayForCash,
            ItemStatus: d?.ItemStatus,
            ItemIsFeeItem: d?.ItemIsFeeItem,
            ItemIsCssRestricted: d?.ItemIsCssRestricted,
            ItemIsFhaRestricted: d?.ItemIsFhaRestricted,
            ItemIsDisplayForCash: d?.ItemIsDisplayForCash,
            ItemIsRestrictOptionPrice: d?.ItemIsRestrictOptionPrice,
            ItemIsRestrictCssCost: d?.ItemIsRestrictCssCost,
            ItemIsHopeRefundsIncluded: d?.ItemIsHopeRefundsIncluded,
            ItemProfitPercentage: d?.ItemProfitPercentage);
    }

    private static FundingResponse? MapFunding(FundingRequestCache? funding)
    {
        if (funding is null) return null;

        return new FundingResponse(
            RefFundingRequestId: funding.RefFundingRequestId,
            FundingRequestStatus: funding.Status.ToString(),
            RequestAmount: funding.RequestAmount,
            LenderId: funding.LenderId,
            LenderName: funding.LenderName,
            ApprovalDate: funding.ApprovalDate,
            ApprovalExpirationDate: funding.ApprovalExpirationDate,
            FundingKeys: MapFundingKeys(funding.FundingKeys));
    }

    private static FundingKeyEntry[]? MapFundingKeys(JsonDocument? fundingKeys)
    {
        if (fundingKeys is null) return null;

        return fundingKeys.RootElement.EnumerateArray()
            .Select(e => new FundingKeyEntry(
                e.TryGetProperty("Key", out var k) ? k.GetString() ?? string.Empty : string.Empty,
                e.TryGetProperty("Value", out var v) ? v.GetString() : null))
            .ToArray();
    }

    internal static string FormatRole(SalesTeamRole role) => role switch
    {
        SalesTeamRole.Primary => "Primary Salesperson",
        SalesTeamRole.Secondary => "Secondary Salesperson",
        _ => role.ToString()
    };

    private static int? ResolveEmployeeNumber(int? authorizedUserId, IReadOnlyDictionary<int, int>? map)
    {
        if (authorizedUserId is null || map is null) return null;
        return map.TryGetValue(authorizedUserId.Value, out var empNum) ? empNum : null;
    }

    private static bool ParseBoolAnswer(string? answer) =>
        string.Equals(answer, "true", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(answer, "Y", StringComparison.OrdinalIgnoreCase) ||
        answer == "1";
}
