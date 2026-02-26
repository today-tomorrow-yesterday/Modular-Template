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
//
// This is the most side-effect-heavy handler in the Sales module. Changing the
// home line can invalidate project costs (different home types allow different
// cost categories), change W&A pricing, and require tax recalculation. The
// 8-step flow below runs these cascades in the correct order:
//   1. Load          — hydrate the full package aggregate
//   2. Snapshot      — capture pre-mutation state for tax change detection
//   3. Upsert        — delete-then-insert the home line (PUT semantics)
//   4. Prune costs   — remove project costs that don't apply to the new home type
//   5. W&A           — recalculate wheel & axle pricing via iSeries
//   6. Clear errors  — wipe stale tax errors so the UI doesn't show ghosts
//   7. Tax flag      — compare before/after and flag if taxes need recalculation
//   8. Finalize      — raise events, recalculate GP, persist
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
        // Step 1: Load package with all lines + sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageHomeResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Snapshot existing state for tax change detection
        var (previousHomeType, taxSnapshot) = SnapshotCurrentState(package);

        // Step 3: Upsert home line (delete-then-insert — PUT semantics)
        var error = await UpsertHomeLine(package, request.Home, cancellationToken);
        if (error is not null)
        {
            return Result.Failure<UpdatePackageHomeResult>(error);
        }

        // Step 4: Remove project costs that are invalid for the new home type
        RemoveInvalidProjectCosts(package, previousHomeType, request.Home.HomeType);

        // Step 5: Recalculate W&A pricing (always — handler knows home changed)
        await RecalculateWheelAndAxlePricing(package, request.Home, cancellationToken);

        // Step 6: Clear tax calculation errors (always)
        ClearTaxErrors(package);

        // Step 7: Flag tax recalculation if any tax-affecting field changed (ALWAYS second-to-last)
        FlagTaxRecalculationIfNeeded(package, taxSnapshot, request.Home);

        // Step 8: Finalize — raise events, recalculate GP, persist
        package.Sale.RaiseSaleSummaryChanged();
        package.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageHomeResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    // --- Step 2: Snapshot existing state ---
    // Captures the pre-mutation state of the package so Step 7 can detect whether
    // tax-affecting fields actually changed. Without this, we'd always flag for
    // tax recalculation even on no-op updates (e.g. re-saving the same home).

    private sealed record TaxChangeSnapshot(
        HomeType? HomeType,
        string? StockNumber,
        decimal HomeSalePrice,
        int ProjectCostCount,
        List<decimal> ProjectCostPrices);

    private static (HomeType? PreviousHomeType, TaxChangeSnapshot Snapshot) SnapshotCurrentState(
        Package package)
    {
        var existingHome = package.Lines.OfType<HomeLine>().SingleOrDefault();

        // Project cost prices are sorted so Step 7 can use SequenceEqual for comparison
        // regardless of insertion order.
        var snapshot = new TaxChangeSnapshot(
            HomeType: existingHome?.Details?.HomeType,
            StockNumber: existingHome?.Details?.StockNumber,
            HomeSalePrice: existingHome?.SalePrice ?? 0m,
            ProjectCostCount: package.Lines.OfType<ProjectCostLine>().Count(),
            ProjectCostPrices: [.. package.Lines
                .OfType<ProjectCostLine>()
                .Select(l => l.SalePrice)
                .OrderBy(p => p)]);

        return (existingHome?.Details?.HomeType, snapshot);
    }

    // --- Step 3: Upsert home line ---
    // PUT semantics: delete the existing home line (if any) then insert the new one.
    // This avoids partial-update bugs — the caller always sends the full home state.
    // Returns null on success, or an Error if validation fails.

    private async Task<Error?> UpsertHomeLine(
        Package package, UpdatePackageHomeRequest home, CancellationToken ct)
    {
        package.RemoveLine<HomeLine>();

        // OnLot homes must reference a row in the inventory cache (synced from iSeries).
        // We resolve the FK here so the HomeLine can link back to the cached inventory
        // record for downstream lookups (e.g. stock-based W&A pricing in Step 5).
        int? onLotHomeId = null;
        if (home.HomeSourceType is HomeSourceType.OnLot)
        {
            var homeCenterNumber = package.Sale.RetailLocation.RefHomeCenterNumber!.Value;
            var cached = await inventoryCacheQueries.FindByHomeCenterAndStockAsync(
                homeCenterNumber, home.StockNumber!, ct);

            if (cached is null)
            {
                return Error.NotFound(
                    "OnLotHome.NotFound",
                    $"On-lot home with stock number '{home.StockNumber}' not found in inventory cache for home center {homeCenterNumber}.");
            }

            onLotHomeId = cached.Id;
        }

        var details = HomeDetails.Create(
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

        package.AddLine(HomeLine.Create(
            packageId: package.Id,
            salePrice: home.SalePrice,
            estimatedCost: home.EstimatedCost,
            retailSalePrice: home.RetailSalePrice,
            responsibility: Responsibility.Seller,
            details: details,
            onLotHomeId: onLotHomeId));

        return null;
    }

    // --- Step 4: Remove invalid project costs ---
    // When the home type changes (e.g. New → Used), certain project cost categories
    // become invalid. For example, "Repo Costs" (cat 12) don't apply to New or Used
    // homes, and "Refurbishment" items (cat 11) don't apply to New homes. This is a
    // carry-over from the legacy system — see PackageDtoExtensions.GetItemsToRemoveByHomeType().
    // Skipped entirely if the home type didn't change (no-op update or first home).

    private static void RemoveInvalidProjectCosts(
        Package package, HomeType? previousHomeType, HomeType newHomeType)
    {
        if (previousHomeType is null || previousHomeType == newHomeType)
        {
            return;
        }

        // PreviouslyTitled is a tax field that depends on home type — reset it so the
        // tax calculation doesn't carry stale data from the old home type.
        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        taxLine?.ClearPreviouslyTitled();

        // W&A project costs are always recalculated from scratch in Step 5,
        // so remove them here to avoid duplicates.
        package.RemoveProjectCost(ProjectCostCategories.WheelsAndAxles, ProjectCostItems.WaRental);
        package.RemoveProjectCost(ProjectCostCategories.WheelsAndAxles, ProjectCostItems.WaPurchase);

        // Remove project costs that are not valid for the new home type.
        // Category/item IDs match the legacy iSeries project cost catalog.
        switch (newHomeType)
        {
            case HomeType.New:
                package.RemoveProjectCost(ProjectCostCategories.Refurbishment, ProjectCostItems.Cleaning);
                package.RemoveProjectCost(ProjectCostCategories.Refurbishment, ProjectCostItems.RepairRefurb);
                package.RemoveProjectCost(ProjectCostCategories.Refurbishment, ProjectCostItems.RefurbParts);
                package.RemoveProjectCost(ProjectCostCategories.Refurbishment, ProjectCostItems.Drapes);
                package.RemoveProjectCostsByCategory(ProjectCostCategories.RepoCosts);
                package.RemoveProjectCost(ProjectCostCategories.MiscellaneousTax, ProjectCostItems.TaxUndercollection);
                break;
            case HomeType.Used:
                package.RemoveProjectCostsByCategory(ProjectCostCategories.RepoCosts);
                package.RemoveProjectCost(ProjectCostCategories.Decorating, ProjectCostItems.DecoratingDrapes);
                package.RemoveProjectCost(ProjectCostCategories.MiscellaneousTax, ProjectCostItems.TaxUndercollection);
                break;
            case HomeType.Repo:
                package.RemoveProjectCost(ProjectCostCategories.Refurbishment, ProjectCostItems.Cleaning);
                package.RemoveProjectCost(ProjectCostCategories.Refurbishment, ProjectCostItems.RepairRefurb);
                package.RemoveProjectCost(ProjectCostCategories.Refurbishment, ProjectCostItems.RefurbParts);
                package.RemoveProjectCost(ProjectCostCategories.Decorating, ProjectCostItems.DecoratingDrapes);
                package.RemoveProjectCost(ProjectCostCategories.MiscellaneousTax, ProjectCostItems.TaxUndercollection);
                break;
        }
    }

    // --- Step 5: Recalculate W&A pricing ---
    // Wheel & Axle pricing depends on the home's physical attributes (stock number,
    // or wheel/axle counts). Since the home just changed, we always remove-then-recalculate.
    // Two iSeries pricing paths: stock-number lookup (OnLot/VmfHomes) or count-based calc.
    // If the user chose no W&A option (null), we just remove and exit.

    private async Task RecalculateWheelAndAxlePricing(
        Package package, UpdatePackageHomeRequest home, CancellationToken cancellationToken)
    {
        // Remove existing W&A project costs — they'll be re-added below if applicable
        package.RemoveProjectCost(ProjectCostCategories.WheelsAndAxles, ProjectCostItems.WaRental);
        package.RemoveProjectCost(ProjectCostCategories.WheelsAndAxles, ProjectCostItems.WaPurchase);

        if (home.WheelAndAxlesOption is null)
        {
            return;
        }

        var (catId, itemId) = home.WheelAndAxlesOption.Value switch
        {
            WheelAndAxlesOption.Rent => (ProjectCostCategories.WheelsAndAxles, ProjectCostItems.WaRental),
            WheelAndAxlesOption.Purchase => (ProjectCostCategories.WheelsAndAxles, ProjectCostItems.WaPurchase),
            _ => (ProjectCostCategories.WheelsAndAxles, ProjectCostItems.WaRental)
        };

        // Calculate W&A price via iSeries — stock number path or wheel/axle count path
        WheelAndAxlePriceResult waResult;
        if (home.HomeSourceType is HomeSourceType.OnLot or HomeSourceType.VmfHomes
            && !string.IsNullOrEmpty(home.StockNumber))
        {
            var homeCenterNumber = package.Sale.RetailLocation.RefHomeCenterNumber ?? 0;
            waResult = await iSeriesAdapter.GetWheelAndAxlePriceByStock(
                new WheelAndAxlePriceByStockRequest
                {
                    HomeCenterNumber = homeCenterNumber,
                    StockNumber = home.StockNumber
                }, cancellationToken);
        }
        else if (home.NumberOfWheels.HasValue && home.NumberOfAxles.HasValue)
        {
            waResult = await iSeriesAdapter.CalculateWheelAndAxlePriceByCount(
                new WheelAndAxlePriceByCountRequest
                {
                    NumberOfWheels = home.NumberOfWheels.Value,
                    NumberOfAxles = home.NumberOfAxles.Value
                }, cancellationToken);
        }
        else
        {
            return;
        }

        if (waResult.SalePrice <= 0)
        {
            return;
        }

        var details = ProjectCostDetails.Create(
            categoryId: catId,
            itemId: itemId,
            itemDescription: home.WheelAndAxlesOption.Value == WheelAndAxlesOption.Rent
                ? "Wheels & Axles - Rental"
                : "Wheels & Axles - Purchase");

        package.AddLine(ProjectCostLine.Create(
            packageId: package.Id,
            salePrice: waResult.SalePrice,
            estimatedCost: waResult.Cost,
            retailSalePrice: waResult.SalePrice,
            responsibility: Responsibility.Seller,
            shouldExcludeFromPricing: false,
            details: details));
    }

    // --- Step 6: Clear tax errors ---
    // Previous tax calculation errors (e.g. from a failed iSeries tax call) become
    // stale when the home changes. Clear them so the UI doesn't show old error messages
    // while awaiting the next tax recalculation.

    private static void ClearTaxErrors(Package package)
    {
        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        if (taxLine?.Details?.Errors is null or [])
        {
            return;
        }

        taxLine.ClearErrors();
    }

    // --- Step 7: Flag tax recalculation if needed ---
    // Compares the pre-mutation snapshot (Step 2) against the current state to decide
    // whether taxes need recalculation. Tax-affecting fields: home type, stock number,
    // home sale price, and project cost count/prices. If nothing changed, we skip —
    // this prevents unnecessary MustRecalculateTaxes flags on no-op home saves.
    // When taxes DO need recalculation, we also remove the existing Use Tax project
    // cost line (cat 9 / item 21) because it was computed from the old tax state.

    private static void FlagTaxRecalculationIfNeeded(
        Package package, TaxChangeSnapshot before, UpdatePackageHomeRequest home)
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

        // Clear cached tax calculations — they're based on the old home state
        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        taxLine?.ClearCalculations();

        // Remove stale Use Tax project cost — will be recomputed on next tax calculation
        package.RemoveProjectCost(
            ProjectCostCategories.UseTax,
            ProjectCostItems.UseTax);
        package.FlagForTaxRecalculation();
    }
}
