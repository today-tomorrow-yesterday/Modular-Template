using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageWarranty;

// Flow: PUT /api/v1/packages/{packageId}/warranty → UpdatePackageWarrantyCommand →
//   upsert WarrantyLine (delete-then-insert — PUT semantics) →
//   recalculates GrossProfit.
// NOTE: Admin override endpoint — direct-writes warranty data without iSeries quote workflow.
// Primary quote path is POST /sales/{saleId}/insurance/quote?type=warranty via GenerateWarrantyQuoteCommandHandler.
internal sealed class UpdatePackageWarrantyCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageWarrantyCommand, UpdatePackageWarrantyResult>
{
    public async Task<Result<UpdatePackageWarrantyResult>> Handle(
        UpdatePackageWarrantyCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with all lines + sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageWarrantyResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Snapshot existing warranty state for tax change detection
        var existingWarranty = package.Lines.OfType<WarrantyLine>().SingleOrDefault();
        var oldAmount = existingWarranty?.Details?.WarrantyAmount;
        var wasSelected = existingWarranty?.Details?.WarrantySelected ?? false;

        // Step 3: Upsert warranty line (delete-then-insert — PUT semantics)
        UpsertWarrantyLine(package, request);

        // Step 4: Tax change detection — flag if amount or selection changed
        if (oldAmount != request.WarrantyAmount || !wasSelected)
        {
            var taxLine = package.Lines.OfType<TaxLine>().SingleOrDefault();
            taxLine?.ClearCalculations();

            package.RemoveProjectCost(9, 21);

            package.FlagForTaxRecalculation();
        }

        // Step 5: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageWarrantyResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    private static void UpsertWarrantyLine(Package package, UpdatePackageWarrantyCommand request)
    {
        package.RemoveWarrantyLine();

        var details = WarrantyDetails.Create(
            warrantyAmount: request.WarrantyAmount,
            salesTaxPremium: 0m, // SalesTaxPremium populated by iSeries warranty quote, not this PUT
            warrantySelected: request.WarrantySelected);

        var newLine = WarrantyLine.Create(
            packageId: package.Id,
            salePrice: request.WarrantyAmount,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            shouldExcludeFromPricing: false,
            details: details);

        package.AddLine(newLine);
    }

}
