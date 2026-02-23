using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageLand;

// Flow: PUT /api/v1/packages/{packageId}/land → UpdatePackageLandCommand →
//   capture pre-update snapshot → upsert LandLine (PUT semantics) →
//   land pricing recalculation (4-branch matrix) → Land Payoff PC sync (Cat 2/Item 1) →
//   tax change detection → recalculates GrossProfit.
internal sealed class UpdatePackageLandCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageLandCommand, UpdatePackageLandResult>
{
    private const int LandPayoffCategoryNumber = 2;
    private const int LandPayoffItemNumber = 1;
    private const int UseTaxCategoryNumber = 9;
    private const int UseTaxItemNumber = 21;

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

        // Step 2: Capture pre-update snapshot for tax change detection
        var existingLandLine = package.Lines.OfType<LandLine>().SingleOrDefault();
        var oldLandSalePrice = existingLandLine?.SalePrice ?? 0m;

        // Step 3: Upsert Land line (PUT semantics — delete old, insert new)
        UpsertLandLine(package, request);

        // Step 4: Land pricing recalculation — overwrite SalePrice/EstimatedCost from detail fields
        var landLine = package.Lines.OfType<LandLine>().Single();
        RecalculateLandPricing(landLine);

        // Step 5: Land Payoff project cost sync — Cat 2 / Item 1
        SyncLandPayoffProjectCost(package, landLine);

        // Step 6: Tax change detection — compare old vs current Land SalePrice
        var newLandSalePrice = landLine.SalePrice;
        if (oldLandSalePrice != newLandSalePrice)
        {
            var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
            taxLine?.ClearCalculations();

            RemoveUseTaxProjectCost(package);

            package.FlagForTaxRecalculation();
        }

        // Step 7: Raise SaleSummaryChanged + save
        // (LandLineUpdatedDomainEvent already raised inside Package.AddLine)
        package.Sale.RaiseSaleSummaryChanged();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageLandResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    private static void UpsertLandLine(Package package, UpdatePackageLandCommand request)
    {
        var existing = package.Lines.OfType<LandLine>().SingleOrDefault();
        if (existing is not null)
        {
            package.RemoveLine(existing);
        }

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

    // Step 4: Reset SalePrice/EstimatedCost to 0, then recalculate from land type.
    // RetailSalePrice stays at the client-provided value.
    private static void RecalculateLandPricing(LandLine landLine)
    {
        if (landLine.Details is null)
        {
            return;
        }

        decimal salePrice = 0m;
        decimal estimatedCost = 0m;
        var details = landLine.Details;

        // 1. CustomerLandPayoff: SalePrice = PayoffAmountFinancing, EstimatedCost = PayoffAmountFinancing
        if (details.LandInclusion == LandInclusion.CustomerLandPayoff)
        {
            salePrice = details.PayoffAmountFinancing ?? 0m;
            estimatedCost = details.PayoffAmountFinancing ?? 0m;
        }
        // 2. LandPurchase: SalePrice = PurchasePrice, EstimatedCost = PurchasePrice
        else if (details.TypeOfLandWanted == TypeOfLandWanted.LandPurchase)
        {
            salePrice = details.PurchasePrice ?? 0m;
            estimatedCost = details.PurchasePrice ?? 0m;
        }
        // 3. HomeCenterOwnedLand: SalePrice = LandSalesPrice, EstimatedCost = LandCost (dealer margin)
        else if (details.TypeOfLandWanted == TypeOfLandWanted.HomeCenterOwnedLand)
        {
            salePrice = details.LandSalesPrice ?? 0m;
            estimatedCost = details.LandCost ?? 0m;
        }
        // 4. All other types: SalePrice and EstimatedCost remain at 0

        landLine.UpdatePricing(salePrice, estimatedCost, landLine.RetailSalePrice);
    }

    // Step 5: Create/update/remove Land Payoff project cost (Cat 2, Item 1).
    // Only exists for priced land types. ShouldExcludeFromPricing = true.
    private static void SyncLandPayoffProjectCost(Package package, LandLine landLine)
    {
        // Remove existing Land Payoff if present
        var existingPayoff = package.Lines
            .OfType<ProjectCostLine>()
            .SingleOrDefault(l =>
                l.Details?.CategoryId == LandPayoffCategoryNumber
                && l.Details?.ItemId == LandPayoffItemNumber);

        if (existingPayoff is not null)
        {
            package.RemoveLine(existingPayoff);
        }

        // Only create for priced land types
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
            categoryId: LandPayoffCategoryNumber,
            itemId: LandPayoffItemNumber,
            itemDescription: "Land Payoff");

        package.AddLine(ProjectCostLine.Create(
            packageId: package.Id,
            salePrice: landLine.SalePrice,
            estimatedCost: landLine.EstimatedCost,
            retailSalePrice: landLine.SalePrice, // Legacy: RetailSalePrice = SalePrice (passes validation)
            responsibility: Responsibility.Seller,
            shouldExcludeFromPricing: true,
            details: details));
    }

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
}
