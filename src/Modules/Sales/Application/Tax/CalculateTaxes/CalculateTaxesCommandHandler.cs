using Modules.Sales.Domain;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Tax;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;
using System.Text.Json;

namespace Modules.Sales.Application.Tax.CalculateTaxes;

// Flow: POST /api/v1/packages/{packageId}/tax → CalculateTaxesCommand (empty body) →
//   guard clauses → read AppId from cache.funding → 4-step iSeries sequence →
//   soft error handling → state-specific nullification → Use Tax PC management →
//   build 6 TaxItems → clear MustRecalculateTaxes.
internal sealed class CalculateTaxesCommandHandler(
    IPackageRepository packageRepository,
    IFundingRequestCacheRepository fundingRequestCacheRepository,
    IiSeriesAdapter iSeriesAdapter,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<CalculateTaxesCommand, CalculateTaxesResult>
{

    public async Task<Result<CalculateTaxesResult>> Handle(
        CalculateTaxesCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with full sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<CalculateTaxesResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        var sale = package.Sale;

        // Step 2: Guard clauses
        var homeLine = package.Lines.OfType<HomeLine>().SingleOrDefault();
        if (homeLine?.Details is null)
        {
            return Result.Failure<CalculateTaxesResult>(Error.Validation(
                "Tax.NoHomeLine", "Cannot calculate taxes without a home line on the package."));
        }

        if (sale.DeliveryAddress is null)
        {
            return Result.Failure<CalculateTaxesResult>(Error.Validation(
                "Tax.NoDeliveryAddress", "Cannot calculate taxes without a delivery address on the sale."));
        }

        if (sale.RetailLocation is null)
        {
            return Result.Failure<CalculateTaxesResult>(Error.Validation(
                "Tax.NoRetailLocation", "Retail location not found."));
        }

        var existingTaxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        if (existingTaxLine?.Details?.PreviouslyTitled is null)
        {
            return Result.Failure<CalculateTaxesResult>(Error.Validation(
                "Tax.NoPreviouslyTitled", "PreviouslyTitled must be set on the tax configuration before calculating."));
        }

        // Read tax config from existing Tax line (saved by Journey A PUT)
        var taxConfig = existingTaxLine.Details;
        var taxExemptionId = taxConfig.TaxExemptionId;

        // Step 3: Read AppId from cache.funding
        var fundingCache = await fundingRequestCacheRepository.GetByPackageIdAsync(
            package.Id, cancellationToken);

        int? appId = null;
        if (fundingCache?.FundingKeys is not null)
        {
            appId = ExtractAppIdFromFundingKeys(fundingCache.FundingKeys);
        }

        if (appId is null)
        {
            return Result.Failure<CalculateTaxesResult>(Error.Validation(
                "Tax.NoAppId", "Cannot extract lender identifiers from funding cache."));
        }

        var homeCenterNumber = sale.RetailLocation.RefHomeCenterNumber ?? 0;
        var stateCode = sale.RetailLocation.StateCode ?? string.Empty;

        // Step 4: Phase 1 — Delete existing question answers on iSeries
        await iSeriesAdapter.DeleteTaxQuestionAnswers(
            new DeleteTaxQuestionAnswersRequest { AppId = appId.Value },
            cancellationToken);

        // Step 5: Phase 2 — Parallel: Update allowances + Insert question answers
        var allowancesTask = UpdateAllowancesAsync(
            appId.Value, homeCenterNumber, package, sale, homeLine,
            taxConfig.PreviouslyTitled, taxExemptionId, cancellationToken);

        // CustomerNumber: iSeries expects it per answer row. Hardcoded to 0
        // matching the allowance wire pattern — iSeries stored proc uses AppId as primary correlation.
        var questionsTask = InsertQuestionAnswersAsync(
            appId.Value, 0, taxConfig.StateTaxQuestionAnswers, cancellationToken);

        await Task.WhenAll(allowancesTask, questionsTask);

        // Step 6: Phase 3 — Calculate tax via iSeries
        var taxResult = await iSeriesAdapter.CalculateTax(
            new TaxCalculationRequest
            {
                HomeCenterNumber = homeCenterNumber,
                AppId = appId.Value,
                StockNumber = (homeLine.Details.StockNumber ?? string.Empty).ToUpperInvariant(),
                ModularClassification = MapModularClassification(homeLine.Details.ModularType),
                WarrantyAmount = package.Lines.OfType<WarrantyLine>().SingleOrDefault()?.SalePrice ?? 0m,
                HomeCondition = MapHomeCondition(homeLine.Details.HomeType),
                NumberOfFloorSections = homeLine.Details.NumberOfFloorSections ?? 0
            },
            cancellationToken);

        // Step 7: Handle soft errors
        var hasErrors = taxResult.Messages is { Count: > 0 };
        if (hasErrors)
        {
            return await HandleSoftErrorAsync(
                package, existingTaxLine, taxResult.Messages!, cancellationToken);
        }

        // Step 8: State-specific post-processing
        decimal? grossReceiptCityTax = stateCode == "TN" ? taxResult.GrossReceiptCityTax : null;
        decimal? grossReceiptCountyTax = stateCode == "TN" ? taxResult.GrossReceiptCountyTax : null;
        decimal? mhit = stateCode == "TX" ? taxResult.ManufacturedHomeInventoryTax : null;

        // Step 9: Use Tax ProjectCost management
        SyncUseTaxProjectCost(package, taxResult.UseTax);

        // Step 10: Build 6 TaxItems (always create all 6)
        var taxItems = new List<TaxItem>
        {
            TaxItem.Create("State Tax", taxResult.StateTax),
            TaxItem.Create("City Tax", taxResult.CityTax),
            TaxItem.Create("County Tax", taxResult.CountyTax),
            CreateNullableTaxItem("Gross Receipt City Tax", grossReceiptCityTax),
            CreateNullableTaxItem("Gross Receipt County Tax", grossReceiptCountyTax),
            CreateNullableTaxItem("Manufactured Home Inventory Tax", mhit)
        };

        var totalTaxSalePrice = taxResult.StateTax + taxResult.CityTax + taxResult.CountyTax
            + (grossReceiptCityTax ?? 0m) + (grossReceiptCountyTax ?? 0m) + (mhit ?? 0m);

        // Step 11: Update Tax line with results
        package.RemoveLine<TaxLine>();

        var updatedTaxDetails = TaxDetails.Create(
            previouslyTitled: taxConfig.PreviouslyTitled,
            taxExemptionId: taxConfig.TaxExemptionId,
            questionAnswers: taxConfig.StateTaxQuestionAnswers,
            taxes: taxItems,
            errors: null,
            taxExemptionDescription: taxConfig.TaxExemptionDescription,
            stateCode: stateCode,
            deliveryCity: sale.DeliveryAddress!.City,
            deliveryCounty: sale.DeliveryAddress.County,
            deliveryPostalCode: sale.DeliveryAddress.PostalCode,
            deliveryIsWithinCityLimits: sale.DeliveryAddress.IsWithinCityLimits);

        package.AddLine(TaxLine.Create(
            packageId: package.Id,
            salePrice: totalTaxSalePrice,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            shouldExcludeFromPricing: false,
            details: updatedTaxDetails));

        // Step 12: Clear recalculation flag
        package.ClearTaxRecalculationFlag();

        // Step 13: Save
        sale.RaiseSaleSummaryChanged();
        package.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CalculateTaxesResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes,
            totalTaxSalePrice,
            taxItems.Select(t => new TaxItemResult(t.Name, t.IsOverridden, t.CalculatedAmount, t.ChargedAmount)).ToList(),
            []);
    }

    // Soft error — save package with empty TaxItems, SalePrice = 0, errors populated
    private async Task<Result<CalculateTaxesResult>> HandleSoftErrorAsync(
        Package package, TaxLine existingTaxLine, List<string> errors, CancellationToken ct)
    {
        var taxConfig = existingTaxLine.Details!;
        var deliveryAddress = package.Sale?.DeliveryAddress;

        package.RemoveLine<TaxLine>();

        var errorTaxDetails = TaxDetails.Create(
            previouslyTitled: taxConfig.PreviouslyTitled,
            taxExemptionId: taxConfig.TaxExemptionId,
            questionAnswers: taxConfig.StateTaxQuestionAnswers,
            taxes: [],
            errors: errors,
            taxExemptionDescription: taxConfig.TaxExemptionDescription,
            stateCode: taxConfig.StateCode ?? package.Sale?.RetailLocation?.StateCode,
            deliveryCity: deliveryAddress?.City,
            deliveryCounty: deliveryAddress?.County,
            deliveryPostalCode: deliveryAddress?.PostalCode,
            deliveryIsWithinCityLimits: deliveryAddress?.IsWithinCityLimits);

        package.AddLine(TaxLine.Create(
            packageId: package.Id,
            salePrice: 0m,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            shouldExcludeFromPricing: false,
            details: errorTaxDetails));

        // Remove Use Tax ProjectCost on error
        package.RemoveProjectCost(ProjectCostCategories.UseTax, ProjectCostItems.UseTax);

        package.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(ct);

        return new CalculateTaxesResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes,
            0m,
            [],
            errors);
    }

    // Extract AppId from FundingKeys JSONB [{"Key":"AppId","Value":"999999"}]
    private static int? ExtractAppIdFromFundingKeys(JsonDocument fundingKeys)
    {
        if (fundingKeys.RootElement.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var element in fundingKeys.RootElement.EnumerateArray())
        {
            if (element.TryGetProperty("Key", out var key) &&
                key.GetString() == "AppId" &&
                element.TryGetProperty("Value", out var value))
            {
                var valueStr = value.GetString();
                if (valueStr is not null && int.TryParse(valueStr, out var appId))
                    return appId;
            }
        }

        return null;
    }

    // Upsert or remove Use Tax ProjectCost (Cat 9, Item 21)
    private static void SyncUseTaxProjectCost(Package package, decimal useTaxAmount)
    {
        package.RemoveProjectCost(ProjectCostCategories.UseTax, ProjectCostItems.UseTax);

        if (useTaxAmount > 0)
        {
            var details = ProjectCostDetails.Create(
                categoryId: ProjectCostCategories.UseTax,
                itemId: ProjectCostItems.UseTax,
                itemDescription: "Use Tax");

            package.AddLine(ProjectCostLine.Create(
                packageId: package.Id,
                salePrice: useTaxAmount,
                estimatedCost: useTaxAmount,
                retailSalePrice: useTaxAmount,
                responsibility: Responsibility.Seller,
                shouldExcludeFromPricing: false,
                details: details));
        }
    }

    // Create TaxItem with nullable amount (for state-specific nullification)
    private static TaxItem CreateNullableTaxItem(string name, decimal? amount)
    {
        return TaxItem.Create(name, amount ?? 0m);
    }

    private async Task UpdateAllowancesAsync(
        int appId, int homeCenterNumber, Package package, Sale sale,
        HomeLine homeLine, string? previouslyTitled, int? taxExemptionId,
        CancellationToken ct)
    {
        var tradeIns = package.Lines.OfType<TradeInLine>().ToList();
        var projectCosts = package.Lines.OfType<ProjectCostLine>()
            .Where(pc => !pc.ShouldExcludeFromPricing).ToList();

        var addOns = projectCosts.Select(pc => new AllowanceAddOn
        {
            CategoryNumber = pc.Details?.CategoryId ?? 0,
            ItemNumber = pc.Details?.ItemId ?? 0,
            Cost = pc.EstimatedCost,
            SalePrice = pc.SalePrice
        }).ToArray();

        await iSeriesAdapter.UpdateAllowances(
            new AllowanceUpdateRequest
            {
                AppId = appId,
                CorrelationId = Guid.NewGuid(),
                HomeCenterNumber = homeCenterNumber,
                HomeSalePrice = homeLine.SalePrice,
                HomeNetInvoice = homeLine.Details?.NetInvoice ?? 0m,
                NumberOfFloorSections = homeLine.Details?.NumberOfFloorSections ?? 0,
                FreightCost = homeLine.Details?.FreightCost ?? 0m,
                CarrierFrameDeposit = homeLine.Details?.CarrierFrameDeposit ?? 0m,
                GrossCost = homeLine.Details?.GrossCost ?? 0m,
                TaxIncludedOnInvoice = homeLine.Details?.TaxIncludedOnInvoice ?? 0m,
                RebateOnMfgInvoice = homeLine.Details?.RebateOnMfgInvoice ?? 0m,
                HomeCondition = MapHomeCondition(homeLine.Details!.HomeType),
                TradeAllowance = tradeIns.Sum(t => t.Details?.TradeAllowance ?? 0m),
                BookInAmount = tradeIns.Sum(t => t.Details?.BookInAmount ?? 0m),
                TradeInType = MapTradeInTypeCode(tradeIns.FirstOrDefault()?.Details?.TradeType),
                PreviouslyTitled = MapPreviouslyTitled(previouslyTitled),
                IsTaxExempt = taxExemptionId is not null and not 0,
                City = sale.DeliveryAddress?.City ?? string.Empty,
                County = sale.DeliveryAddress?.County ?? string.Empty,
                PostalCode = sale.DeliveryAddress?.PostalCode ?? string.Empty,
                IsWithinCityLimits = sale.DeliveryAddress?.IsWithinCityLimits ?? false,
                PointOfSaleZip = sale.RetailLocation.Zip ?? string.Empty,
                TotalAddOnCost = projectCosts.Sum(pc => pc.EstimatedCost),
                TotalAddOnSalePrice = projectCosts.Sum(pc => pc.SalePrice),
                AddOns = addOns
            },
            ct);
    }

    private async Task InsertQuestionAnswersAsync(
        int appId, int customerNumber, List<Domain.Packages.Details.TaxQuestionAnswer> answers, CancellationToken ct)
    {
        if (answers.Count == 0) return;

        await iSeriesAdapter.InsertTaxQuestionAnswers(
            new InsertTaxQuestionAnswersRequest
            {
                Answers = answers.Select(a => new Rtl.Core.Application.Adapters.ISeries.Tax.TaxQuestionAnswer
                {
                    AppId = appId,
                    CustomerNumber = customerNumber,
                    QuestionNumber = a.QuestionNumber,
                    Answer = a.Answer == "Y"
                }).ToList()
            },
            ct);
    }

    private static HomeCondition MapHomeCondition(HomeType homeType) => homeType switch
    {
        HomeType.New => HomeCondition.New,
        HomeType.Used => HomeCondition.Used,
        HomeType.Repo => HomeCondition.Repo,
        _ => HomeCondition.New
    };

    private static ModularClassification MapModularClassification(ModularType? modularType) => modularType switch
    {
        ModularType.Hud => ModularClassification.Hud,
        ModularType.OnFrame => ModularClassification.OnFrame,
        ModularType.Mod => ModularClassification.OffFrame,
        ModularType.OffFrame => ModularClassification.OffFrame,
        _ => ModularClassification.Hud
    };

    // T-001: Legacy sends empty string when PreviouslyTitled is "No" —
    // iSeries interprets empty string as "not previously titled".
    private static string MapPreviouslyTitled(string? previouslyTitled) =>
        string.Equals(previouslyTitled, "No", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : previouslyTitled ?? string.Empty;

    // T-002: Legacy maps trade-in type names to single-char iSeries codes via a lookup table.
    // The naive tt[0] approach gives wrong codes for "Modular Home" ('M' instead of 'D')
    // and "Motorcycle" ('M' instead of 'C').
    private static char? MapTradeInTypeCode(string? tradeType) => tradeType switch
    {
        "Single Wide" => 'S',
        "Double Wide" => 'D',
        "Modular Home" => 'D',
        "Motorcycle" => 'C',
        "Boat" => 'B',
        "Motor Vehicle" => 'V',
        "Travel Trailer" => 'T',
        "5th Wheel" or "Fifth Wheel" => 'F',
        _ when tradeType is { Length: > 0 } => tradeType[0],
        _ => null
    };
}
