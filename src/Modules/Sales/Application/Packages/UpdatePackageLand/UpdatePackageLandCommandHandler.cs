using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageLand;

// Flow: PUT /api/v1/packages/{packageId}/land → UpdatePackageLandCommand →
//   upsert LandLine + cascade (pricing recalc, Land Payoff sync, tax detection, gross profit) →
//   raises LandLineUpdatedDomainEvent + SaleSummaryChangedDomainEvent
//
// Land pricing is not taken directly from the request — it's derived from the land
// detail fields via a 4-branch matrix (Step 4). The request's SalePrice/EstimatedCost
// are just starting values that get overwritten. The 7-step flow below runs these
// cascades in the correct order:
//   1. Load          — hydrate the full package aggregate
//   2. Snapshot      — capture land sale price for tax change detection
//   3. Upsert        — delete-then-insert the land line (PUT semantics)
//   4. Reprice       — overwrite SalePrice/EstimatedCost from land type matrix
//   5. Payoff sync   — keep the Land Payoff project cost in sync with land pricing
//   6. Tax flag      — compare before/after and flag if taxes need recalculation
//   7. Finalize      — raise events, recalculate GP, persist
internal sealed class UpdatePackageLandCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageLandCommand, UpdatePackageLandResult>
{
    public async Task<Result<UpdatePackageLandResult>> Handle(
        UpdatePackageLandCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with all lines + sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageLandResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Snapshot existing land sale price for tax change detection
        var existingLandLine = package.Lines.OfType<LandLine>().SingleOrDefault();
        var oldLandSalePrice = existingLandLine?.SalePrice ?? 0m;

        // Step 3: Upsert Land line (delete-then-insert — PUT semantics)
        UpsertLandLine(package, request);

        // Step 4: Recalculate land pricing from detail fields
        var landLine = package.Lines.OfType<LandLine>().Single();
        RecalculateLandPricing(landLine);

        // Step 5: Sync Land Payoff project cost (Cat 2 / Item 1)
        SyncLandPayoffProjectCost(package, landLine);

        // Step 6: Flag tax recalculation if land sale price changed
        FlagTaxRecalculationIfNeeded(package, oldLandSalePrice, landLine.SalePrice);

        // Step 7: Finalize — raise events, recalculate GP, persist
        // (LandLineUpdatedDomainEvent already raised inside Package.AddLine)
        package.Sale.RaiseSaleSummaryChanged();
        package.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageLandResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    // --- Step 3: Upsert land line ---
    // PUT semantics: delete the existing land line (if any) then insert the new one.
    // Enum parsing is done here (not in the command) because the API layer sends strings.

    private static void UpsertLandLine(Package package, UpdatePackageLandCommand request)
    {
        package.RemoveLine<LandLine>();

        var landPurchaseType = Enum.Parse<LandPurchaseType>(request.LandPurchaseType);

        var details = LandDetails.Create(
            landPurchaseType: landPurchaseType,
            customerLandType: request.CustomerLandType is not null
                ? Enum.Parse<CustomerLandType>(request.CustomerLandType)
                : null,
            landInclusion: request.LandInclusion is not null
                ? Enum.Parse<LandInclusion>(request.LandInclusion)
                : null,
            typeOfLandWanted: request.TypeOfLandWanted is not null
                ? Enum.Parse<TypeOfLandWanted>(request.TypeOfLandWanted)
                : null,
            estimatedValue: request.EstimatedValue,
            sizeInAcres: request.SizeInAcres,
            propertyOwner: request.PropertyOwner,
            financedBy: request.FinancedBy,
            payoffAmountFinancing: request.PayoffAmountFinancing,
            landEquity: request.LandEquity,
            originalPurchaseDate: request.OriginalPurchaseDate.HasValue
                ? new DateTimeOffset(request.OriginalPurchaseDate.Value, TimeSpan.Zero)
                : null,
            originalPurchasePrice: request.OriginalPurchasePrice,
            propertyOwnerPhoneNumber: request.PropertyOwnerPhoneNumber,
            propertyLotRent: request.PropertyLotRent,
            realtor: request.Realtor,
            purchasePrice: request.PurchasePrice,
            landStockNumber: request.LandStockNumber,
            landCost: request.LandCost,
            landSalesPrice: request.LandSalesPrice,
            communityNumber: request.CommunityNumber,
            communityName: request.CommunityName,
            communityManagerName: request.CommunityManagerName,
            communityManagerPhoneNumber: request.CommunityManagerPhoneNumber,
            communityManagerEmail: request.CommunityManagerEmail,
            communityMonthlyCost: request.CommunityMonthlyCost);

        var newLandLine = LandLine.Create(
            packageId: package.Id,
            salePrice: request.SalePrice,
            estimatedCost: request.EstimatedCost,
            retailSalePrice: request.RetailSalePrice,
            responsibility: Responsibility.Seller,
            details: details);

        package.AddLine(newLandLine);
    }

    // --- Step 4: Recalculate land pricing ---
    // The request's SalePrice is just a starting value — the real price comes from the
    // land type. The 4-branch matrix maps land type to pricing source:
    //   CustomerLandPayoff  → PayoffAmountFinancing for both
    //   LandPurchase        → PurchasePrice for both
    //   HomeCenterOwnedLand → LandSalesPrice / LandCost (dealer margin)
    //   Everything else     → 0 / 0
    // RetailSalePrice stays at the client-provided value.

    private static void RecalculateLandPricing(LandLine landLine)
    {
        if (landLine.Details is null)
        {
            return;
        }

        var salePrice = 0m;
        var estimatedCost = 0m;
        var details = landLine.Details;

        if (details.LandInclusion == LandInclusion.CustomerLandPayoff)
        {
            salePrice = details.PayoffAmountFinancing ?? 0m;
            estimatedCost = details.PayoffAmountFinancing ?? 0m;
        }
        else if (details.TypeOfLandWanted == TypeOfLandWanted.LandPurchase)
        {
            salePrice = details.PurchasePrice ?? 0m;
            estimatedCost = details.PurchasePrice ?? 0m;
        }
        else if (details.TypeOfLandWanted == TypeOfLandWanted.HomeCenterOwnedLand)
        {
            // Dealer margin: cost and sale price differ
            salePrice = details.LandSalesPrice ?? 0m;
            estimatedCost = details.LandCost ?? 0m;
        }

        landLine.UpdatePricing(salePrice, estimatedCost, landLine.RetailSalePrice);
    }

    // --- Step 5: Sync Land Payoff project cost ---
    // A shadow project cost line (Cat 2, Item 1) that mirrors the land line's pricing.
    // Excluded from GP (ShouldExcludeFromPricing = true) but used by commission/funding.
    // Only exists for priced land types with a positive sale price.

    private static void SyncLandPayoffProjectCost(Package package, LandLine landLine)
    {
        // Remove existing, then re-add if applicable (same remove-then-add pattern as W&A)
        package.RemoveProjectCost(ProjectCostCategories.LandPayoff, ProjectCostItems.LandPayoff);

        if (landLine.Details is null)
        {
            return;
        }

        var isPricedType =
            landLine.Details.LandInclusion == LandInclusion.CustomerLandPayoff
            || landLine.Details.TypeOfLandWanted == TypeOfLandWanted.LandPurchase
            || landLine.Details.TypeOfLandWanted == TypeOfLandWanted.HomeCenterOwnedLand;

        if (!isPricedType || landLine.SalePrice <= 0)
        {
            return;
        }

        var details = ProjectCostDetails.Create(
            categoryId: ProjectCostCategories.LandPayoff,
            itemId: ProjectCostItems.LandPayoff,
            itemDescription: "Land Payoff");

        package.AddLine(ProjectCostLine.Create(
            packageId: package.Id,
            salePrice: landLine.SalePrice,
            estimatedCost: landLine.EstimatedCost,
            retailSalePrice: landLine.SalePrice,
            responsibility: Responsibility.Seller,
            shouldExcludeFromPricing: true,
            details: details));
    }

    // --- Step 6: Flag tax recalculation if needed ---
    // Only the land sale price affects taxes. If it didn't change, skip — this prevents
    // unnecessary MustRecalculateTaxes flags on no-op saves. When it does change, clear
    // cached tax calculations and remove the stale Use Tax project cost.

    private static void FlagTaxRecalculationIfNeeded(
        Package package, decimal oldSalePrice, decimal newSalePrice)
    {
        if (oldSalePrice == newSalePrice)
        {
            return;
        }

        var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
        taxLine?.ClearCalculations();

        // Remove stale Use Tax project cost — will be recomputed on next tax calculation
        package.RemoveProjectCost(ProjectCostCategories.UseTax, ProjectCostItems.UseTax);
        package.FlagForTaxRecalculation();
    }
}
