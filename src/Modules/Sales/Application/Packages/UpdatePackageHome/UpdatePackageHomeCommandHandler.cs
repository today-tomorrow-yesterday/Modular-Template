using Modules.Sales.Domain;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageHome;

// Flow: PUT /api/v1/packages/{packageId}/home → UpdatePackageHomeCommand →
//   upsert HomeLine + cascade (tax, project costs, W&A, gross profit) →
//   raises HomeLineUpdatedDomainEvent + SaleSummaryChangedDomainEvent
internal sealed class UpdatePackageHomeCommandHandler(
    IPackageRepository packageRepository,
    IInventoryCacheQueries inventoryCacheQueries,
    IiSeriesAdapter iSeriesAdapter,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageHomeCommand, UpdatePackageHomeResult>
{
    public async Task<Result<UpdatePackageHomeResult>> Handle(
        UpdatePackageHomeCommand request,
        CancellationToken cancellationToken)
    {
        var home = request.Home;

        // Step 1: Load package with all lines + sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageHomeResult>(PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Snapshot existing state for diff
        var existingHome = package.Lines.OfType<HomeLine>().SingleOrDefault();
        var oldDetails = existingHome?.Details;
        var taxSnapshot = TakeTaxSnapshot(package, existingHome);

        // Step 3: Upsert home line (delete-then-insert — PUT semantics)
        if (existingHome is not null)
        {
            package.RemoveLine(existingHome);
        }

        // Resolve inventory cache FK for OnLot homes
        int? onLotHomeId = null;
        if (home.HomeSourceType is HomeSourceType.OnLot)
        {
            var homeCenterNumber = package.Sale.RetailLocation.RefHomeCenterNumber!.Value;
            var cached = await inventoryCacheQueries.FindByHomeCenterAndStockAsync(
                homeCenterNumber, home.StockNumber!, cancellationToken);

            if (cached is null)
            {
                return Result.Failure<UpdatePackageHomeResult>(Error.NotFound(
                    "OnLotHome.NotFound",
                    $"On-lot home with stock number '{home.StockNumber}' not found in inventory cache for home center {homeCenterNumber}."));
            }

            onLotHomeId = cached.Id;
        }

        var newDetails = MapToHomeDetails(home);
        var newHomeLine = HomeLine.Create(
            packageId: package.Id,
            salePrice: home.SalePrice,
            estimatedCost: home.EstimatedCost,
            retailSalePrice: home.RetailSalePrice,
            responsibility: Responsibility.Seller,
            details: newDetails,
            onLotHomeId: onLotHomeId);

        package.AddLine(newHomeLine);

        // Step 4: Home type change cascade (conditional — only when type actually changed)
        if (oldDetails is not null && oldDetails.HomeType != home.HomeType)
        {
            ClearPreviouslyTitled(package);
            await RemoveInvalidProjectCosts(package, home.HomeType, cancellationToken);
        }

        // Step 5: W&A recalculation (always — handler knows home changed)
        await RecalculateWheelAndAxle(package, home, cancellationToken);

        // Step 6: Clear tax calculation errors (always)
        ClearTaxErrors(package);

        // Step 7: Tax change detection (SECOND-TO-LAST — always)
        await DetectTaxChanges(package, taxSnapshot, home, cancellationToken);

        // Raise SaleSummaryChangedDomainEvent
        // Domain event handler publishes SaleSummaryChanged integration event → EventBridge → Inventory
        package.Sale.RaiseSaleSummaryChanged();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageHomeResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    // --- Step 4 helpers ---

    private static void ClearPreviouslyTitled(Package package)
    {
        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        taxLine?.ClearPreviouslyTitled();
    }

    private const int WaRentalCategoryNumber = 1;
    private const int WaRentalItemNumber = 28;
    private const int WaPurchaseCategoryNumber = 1;
    private const int WaPurchaseItemNumber = 29;

    // Home-type-specific project cost removal matrix (legacy: GetItemsToRemoveByHomeType)
    // Aligned with legacy PackageDtoExtensions.cs removal rules:
    //   New:  Cat 11 items 1-4 (Refurb), Cat 12 (all Repo), Cat 13/98 (Tax Undercollection)
    //   Used: Cat 12 (all Repo), Cat 15/4 (Drapes), Cat 13/98 (Tax Undercollection)
    //   Repo: Cat 11 items 1-3 (Refurb excl. Drapes), Cat 15/4 (Drapes), Cat 13/98 (Tax Undercollection)
    private const int RefurbCategoryNumber = 11;
    private const int CleaningItemNumber = 1;
    private const int RepairRefurbItemNumber = 2;
    private const int RefurbPartsItemNumber = 3;
    private const int DrapesItemNumber = 4;
    private const int RepoCostsCategoryNumber = 12;
    private const int MiscTaxCategoryNumber = 13;
    private const int MiscTaxItemNumber = 98;
    private const int DecoratingCategoryNumber = 15;

    private Task RemoveInvalidProjectCosts(
        Package package, HomeType newHomeType, CancellationToken ct)
    {
        // Remove existing W&A project costs — they will be recalculated
        RemoveProjectCostByKey(package, WaRentalCategoryNumber, WaRentalItemNumber);
        RemoveProjectCostByKey(package, WaPurchaseCategoryNumber, WaPurchaseItemNumber);

        // Remove home-type-specific project costs that are invalid for the new type
        switch (newHomeType)
        {
            case HomeType.New:
                // Cat 11 items 1-4 (all refurb items including drapes)
                RemoveProjectCostByKey(package, RefurbCategoryNumber, CleaningItemNumber);
                RemoveProjectCostByKey(package, RefurbCategoryNumber, RepairRefurbItemNumber);
                RemoveProjectCostByKey(package, RefurbCategoryNumber, RefurbPartsItemNumber);
                RemoveProjectCostByKey(package, RefurbCategoryNumber, DrapesItemNumber);
                // Cat 12 (all repo costs)
                RemoveProjectCostsByCategory(package, RepoCostsCategoryNumber);
                // Cat 13/98 (Tax Undercollection)
                RemoveProjectCostByKey(package, MiscTaxCategoryNumber, MiscTaxItemNumber);
                break;
            case HomeType.Used:
                // Cat 12 (all repo costs)
                RemoveProjectCostsByCategory(package, RepoCostsCategoryNumber);
                // Cat 15/4 (Drapes)
                RemoveProjectCostByKey(package, DecoratingCategoryNumber, DrapesItemNumber);
                // Cat 13/98 (Tax Undercollection)
                RemoveProjectCostByKey(package, MiscTaxCategoryNumber, MiscTaxItemNumber);
                break;
            case HomeType.Repo:
                // Cat 11 items 1-3 (refurb — NOT item 4/Drapes for Repo)
                RemoveProjectCostByKey(package, RefurbCategoryNumber, CleaningItemNumber);
                RemoveProjectCostByKey(package, RefurbCategoryNumber, RepairRefurbItemNumber);
                RemoveProjectCostByKey(package, RefurbCategoryNumber, RefurbPartsItemNumber);
                // Cat 15/4 (Drapes)
                RemoveProjectCostByKey(package, DecoratingCategoryNumber, DrapesItemNumber);
                // Cat 13/98 (Tax Undercollection)
                RemoveProjectCostByKey(package, MiscTaxCategoryNumber, MiscTaxItemNumber);
                break;
        }

        return Task.CompletedTask;
    }

    private async Task RecalculateWheelAndAxle(
        Package package, UpdatePackageHomeRequest home, CancellationToken ct)
    {
        // Remove existing W&A project costs
        RemoveProjectCostByKey(package, WaRentalCategoryNumber, WaRentalItemNumber);
        RemoveProjectCostByKey(package, WaPurchaseCategoryNumber, WaPurchaseItemNumber);

        if (home.WheelAndAxlesOption is null)
        {
            return;
        }

        // Determine category/item for the selected W&A option
        var (catId, itemId) = home.WheelAndAxlesOption.Value switch
        {
            WheelAndAxlesOption.Rent => (WaRentalCategoryNumber, WaRentalItemNumber),
            WheelAndAxlesOption.Purchase => (WaPurchaseCategoryNumber, WaPurchaseItemNumber),
            _ => (WaRentalCategoryNumber, WaRentalItemNumber)
        };

        // Calculate W&A price via iSeries — use stock number if OnLot/VmfHomes, otherwise wheel/axle counts
        decimal waPrice;
        if (home.HomeSourceType is HomeSourceType.OnLot or HomeSourceType.VmfHomes
            && !string.IsNullOrEmpty(home.StockNumber))
        {
            var homeCenterNumber = package.Sale.RetailLocation.RefHomeCenterNumber ?? 0;
            waPrice = await iSeriesAdapter.GetWheelAndAxlePriceByStock(
                new WheelAndAxlePriceByStockRequest
                {
                    HomeCenterNumber = homeCenterNumber,
                    StockNumber = home.StockNumber
                }, ct);
        }
        else if (home.NumberOfWheels.HasValue && home.NumberOfAxles.HasValue)
        {
            waPrice = await iSeriesAdapter.CalculateWheelAndAxlePriceByCount(
                new WheelAndAxlePriceByCountRequest
                {
                    NumberOfWheels = home.NumberOfWheels.Value,
                    NumberOfAxles = home.NumberOfAxles.Value
                }, ct);
        }
        else
        {
            return; // Cannot calculate without wheel/axle data
        }

        if (waPrice <= 0)
        {
            return;
        }

        // Create W&A project cost line
        var details = ProjectCostDetails.Create(
            categoryId: catId,
            itemId: itemId,
            itemDescription: home.WheelAndAxlesOption.Value == WheelAndAxlesOption.Rent
                ? "Wheels & Axles - Rental"
                : "Wheels & Axles - Purchase");

        package.AddLine(ProjectCostLine.Create(
            packageId: package.Id,
            salePrice: waPrice,
            estimatedCost: waPrice,
            retailSalePrice: waPrice,
            responsibility: Responsibility.Seller,
            shouldExcludeFromPricing: false,
            details: details));
    }

    private static void RemoveProjectCostsByCategory(Package package, int categoryId)
    {
        var lines = package.Lines
            .OfType<ProjectCostLine>()
            .Where(l => l.Details?.CategoryId == categoryId)
            .ToList();

        foreach (var line in lines)
        {
            package.RemoveLine(line);
        }
    }

    private static void RemoveProjectCostByKey(Package package, int categoryId, int itemId)
    {
        var line = package.Lines
            .OfType<ProjectCostLine>()
            .SingleOrDefault(l =>
                l.Details?.CategoryId == categoryId
                && l.Details?.ItemId == itemId);

        if (line is not null)
        {
            package.RemoveLine(line);
        }
    }

    // --- Step 6: Clear tax errors ---

    private static void ClearTaxErrors(Package package)
    {
        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        if (taxLine?.Details?.Errors is null or [])
        {
            return;
        }

        taxLine.ClearErrors();
    }

    // --- Step 7: Tax change detection ---

    private sealed record TaxSnapshot(
        HomeType? HomeType,
        string? StockNumber,
        decimal HomeSalePrice,
        int ProjectCostCount,
        List<decimal> ProjectCostPrices);

    private static TaxSnapshot TakeTaxSnapshot(Package package, HomeLine? existingHome)
    {
        return new TaxSnapshot(
            HomeType: existingHome?.Details?.HomeType,
            StockNumber: existingHome?.Details?.StockNumber,
            HomeSalePrice: existingHome?.SalePrice ?? 0m,
            ProjectCostCount: package.Lines.OfType<ProjectCostLine>().Count(),
            ProjectCostPrices: package.Lines
                .OfType<ProjectCostLine>()
                .Select(l => l.SalePrice)
                .OrderBy(p => p)
                .ToList());
    }

    private async Task DetectTaxChanges(
        Package package, TaxSnapshot before, UpdatePackageHomeRequest home, CancellationToken ct)
    {
        var currentProjectCosts = package.Lines.OfType<ProjectCostLine>().ToList();

        var changed = before.HomeType != home.HomeType
            || !string.Equals(before.StockNumber, home.StockNumber, StringComparison.OrdinalIgnoreCase)
            || before.HomeSalePrice != home.SalePrice
            || before.ProjectCostCount != currentProjectCosts.Count
            || !before.ProjectCostPrices.SequenceEqual(
                currentProjectCosts.Select(l => l.SalePrice).OrderBy(p => p));

        if (!changed)
        {
            return;
        }

        // Clear existing tax calculation results
        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        taxLine?.ClearCalculations();

        // Remove Use Tax project cost (Cat 9, Item 21)
        RemoveUseTaxProjectCost(package);

        // Signal that taxes must be recalculated
        package.FlagForTaxRecalculation();
    }

    // Use Tax — auto-generated project cost (Cat 9, Item 21)
    private const int UseTaxCategoryNumber = 9;
    private const int UseTaxItemNumber = 21;

    private static void RemoveUseTaxProjectCost(Package package)
    {
        var useTaxPc = package.Lines
            .OfType<ProjectCostLine>()
            .SingleOrDefault(l =>
                l.Details?.CategoryId == UseTaxCategoryNumber
                && l.Details?.ItemId == UseTaxItemNumber);

        if (useTaxPc is not null)
        {
            package.RemoveLine(useTaxPc);
        }
    }

    // --- Mapping ---

    private static HomeDetails MapToHomeDetails(UpdatePackageHomeRequest home)
    {
        return HomeDetails.Create(
            homeType: home.HomeType,
            homeSourceType: home.HomeSourceType,
            stockNumber: home.StockNumber,
            modularType: home.ModularType,
            vendor: home.Vendor,
            make: home.Make,
            model: home.Model,
            modelYear: home.ModelYear,
            lengthInFeet: home.LengthInFeet,
            widthInFeet: home.WidthInFeet,
            bedrooms: home.Bedrooms,
            bathrooms: home.Bathrooms,
            squareFootage: home.SquareFootage,
            serialNumbers: home.SerialNumbers,
            baseCost: home.BaseCost,
            optionsCost: home.OptionsCost,
            freightCost: home.FreightCost,
            invoiceCost: home.InvoiceCost,
            netInvoice: home.NetInvoice,
            grossCost: home.GrossCost,
            taxIncludedOnInvoice: home.TaxIncludedOnInvoice,
            numberOfWheels: home.NumberOfWheels,
            numberOfAxles: home.NumberOfAxles,
            wheelAndAxlesOption: home.WheelAndAxlesOption,
            numberOfFloorSections: home.NumberOfFloorSections,
            carrierFrameDeposit: home.CarrierFrameDeposit,
            rebateOnMfgInvoice: home.RebateOnMfgInvoice,
            claytonBuilt: home.ClaytonBuilt,
            buildType: home.BuildType,
            inventoryReferenceId: home.InventoryReferenceId,
            stateAssociationAndMhiDues: home.StateAssociationAndMhiDues,
            partnerAssistance: home.PartnerAssistance,
            distanceMiles: home.DistanceMiles,
            propertyType: home.PropertyType,
            purchaseOption: home.PurchaseOption,
            listingPrice: home.ListingPrice,
            accountNumber: home.AccountNumber,
            displayAccountId: home.DisplayAccountId,
            streetAddress: home.StreetAddress,
            city: home.City,
            state: home.State,
            zipCode: home.ZipCode);
    }
}
