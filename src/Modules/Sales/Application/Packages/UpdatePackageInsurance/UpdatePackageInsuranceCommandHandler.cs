using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.UpdatePackageInsurance;

// Flow: PUT /api/v1/packages/{packageId}/insurance → UpdatePackageInsuranceCommand →
//   upsert InsuranceLine (delete-then-insert — PUT semantics) →
//   recalculates GrossProfit.
// NOTE: Admin override endpoint — direct-writes insurance data without iSeries quote workflow.
// Primary quote path is POST /sales/{saleId}/insurance/quote?type=homefirst via GenerateHomeFirstQuoteCommandHandler.
internal sealed class UpdatePackageInsuranceCommandHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageInsuranceCommand, UpdatePackageInsuranceResult>
{
    public async Task<Result<UpdatePackageInsuranceResult>> Handle(
        UpdatePackageInsuranceCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with all lines + sale context
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageInsuranceResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Upsert insurance line (delete-then-insert — PUT semantics)
        UpsertInsuranceLine(package, request);

        // Step 3: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageInsuranceResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

    private static void UpsertInsuranceLine(Package package, UpdatePackageInsuranceCommand request)
    {
        // Remove existing insurance line (1:1 section, not 1:many)
        var existing = package.Lines.OfType<InsuranceLine>().SingleOrDefault();
        if (existing is not null)
            package.RemoveLine(existing);

        var insuranceType = Enum.Parse<InsuranceType>(request.InsuranceType);

        var details = InsuranceDetails.Create(
            insuranceType: insuranceType,
            coverageAmount: request.CoverageAmount,
            hasFoundationOrMasonry: request.HasFoundationOrMasonry,
            inParkOrSubdivision: request.InParkOrSubdivision,
            isLandOwnedByCustomer: request.IsLandOwnedByCustomer,
            isPremiumFinanced: request.IsPremiumFinanced,
            quoteId: null, // insurance_quotes table eliminated (v3.37) — no longer referenced
            companyName: request.CompanyName,
            maxCoverage: request.MaxCoverage,
            totalPremium: request.TotalPremium);

        var newLine = InsuranceLine.Create(
            packageId: package.Id,
            salePrice: request.TotalPremium,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Buyer,
            shouldExcludeFromPricing: false,
            details: details);

        package.AddLine(newLine);
    }
}
