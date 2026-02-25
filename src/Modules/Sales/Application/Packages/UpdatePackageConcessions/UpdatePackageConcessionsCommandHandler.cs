using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageConcessions;

// Flow: PUT /api/v1/packages/{packageId}/concessions -> UpdatePackageConcessionsCommand ->
//   upsert CreditLine(Concessions) + sync Seller Paid Closing Cost project cost +
//   tax change detection + recalculate gross profit ->
//   returns updated GP/CGP/MustRecalculateTaxes
internal sealed class UpdatePackageConcessionsCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageConcessionsCommand, UpdatePackageConcessionsResult>
{
    // Auto-generated project cost natural keys (iSeries CATID/ITEMID via cdc.project_cost_item)
    private const int SellerPaidClosingCostCategoryNumber = 14;
    private const int SellerPaidClosingCostItemNumber = 1;
    private const int UseTaxCategoryNumber = 9;
    private const int UseTaxItemNumber = 21;

    public async Task<Result<UpdatePackageConcessionsResult>> Handle(
        UpdatePackageConcessionsCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with all lines + sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageConcessionsResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Capture pre-update snapshot for tax change detection
        var oldNonExcludedCount = package.Lines.Count(l => !l.ShouldExcludeFromPricing);

        // Step 3: Upsert concession line (PUT semantics — delete old, insert new)
        package.RemoveConcessionLine();

        if (request.Amount > 0)
        {
            package.AddLine(CreditLine.CreateConcession(package.Id, request.Amount));
        }

        // Step 4: Seller Paid Closing Cost project cost sync (Cat 14, Item 1)
        SyncSellerPaidClosingCost(package, request.Amount);

        // Step 5: Tax change detection — compare old vs current non-excluded line count
        var newNonExcludedCount = package.Lines.Count(l => !l.ShouldExcludeFromPricing);

        if (oldNonExcludedCount != newNonExcludedCount)
        {
            var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
            taxLine?.ClearCalculations();

            package.RemoveProjectCost(UseTaxCategoryNumber, UseTaxItemNumber);

            package.FlagForTaxRecalculation();
        }

        // Step 6: GrossProfit recalculated automatically by Package.AddLine/RemoveLine
        // Step 7: Save
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageConcessionsResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    private static void SyncSellerPaidClosingCost(Package package, decimal concessionAmount)
    {
        package.RemoveProjectCost(SellerPaidClosingCostCategoryNumber, SellerPaidClosingCostItemNumber);

        if (concessionAmount > 0)
        {
            var details = ProjectCostDetails.Create(
                categoryId: SellerPaidClosingCostCategoryNumber,
                itemId: SellerPaidClosingCostItemNumber,
                itemDescription: "Seller Paid Closing Cost");

            package.AddLine(ProjectCostLine.Create(
                packageId: package.Id,
                salePrice: 0m,
                estimatedCost: concessionAmount,
                retailSalePrice: 0m,
                responsibility: Responsibility.Seller,
                shouldExcludeFromPricing: false,
                details: details));
        }
    }

}
