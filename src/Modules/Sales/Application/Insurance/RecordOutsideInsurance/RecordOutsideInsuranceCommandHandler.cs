using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Insurance.RecordOutsideInsurance;

// Flow: POST /sales/{saleId}/insurance/quote?type=outside -> RecordOutsideInsuranceCommand ->
//   upsert InsuranceLine with InsuranceType.Outside on the sale's primary package ->
//   returns 201 Created (no body -- data echoed back = data caller sent).
internal sealed class RecordOutsideInsuranceCommandHandler(
    ISaleRepository saleRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<RecordOutsideInsuranceCommand>
{
    public async Task<Result> Handle(
        RecordOutsideInsuranceCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale with packages
        var sale = await saleRepository.GetByPublicIdWithFullContextAsync(
            request.SalePublicId, cancellationToken);

        if (sale is null)
            return Result.Failure(SaleErrors.NotFoundByPublicId(request.SalePublicId));

        // Step 2: Find primary package
        var package = sale.Packages.FirstOrDefault(p => p.IsPrimaryPackage);
        if (package is null)
            return Result.Failure(PackageErrors.NoPrimaryPackage());

        // Step 3: Upsert outside insurance line (remove-then-add -- PUT semantics)
        UpsertOutsideInsuranceLine(package, request, sale);

        // Step 4: Persist
        package.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static void UpsertOutsideInsuranceLine(Package package, RecordOutsideInsuranceCommand request, Sale sale)
    {
        package.RemoveOutsideInsuranceLine();

        var homeDetails = package.Lines.OfType<HomeLine>().SingleOrDefault()?.Details;
        var deliveryAddress = sale.DeliveryAddress;

        var details = InsuranceDetails.Create(
            insuranceType: InsuranceType.Outside,
            coverageAmount: request.CoverageAmount,
            providerName: request.ProviderName,
            totalPremium: request.PremiumAmount,
            homeStockNumber: homeDetails?.StockNumber,
            homeModelYear: homeDetails?.ModelYear,
            homeLengthInFeet: homeDetails?.LengthInFeet,
            homeWidthInFeet: homeDetails?.WidthInFeet,
            homeCondition: homeDetails?.HomeType.ToString(),
            deliveryState: deliveryAddress?.State,
            deliveryPostalCode: deliveryAddress?.PostalCode,
            deliveryCity: deliveryAddress?.City,
            deliveryIsWithinCityLimits: deliveryAddress?.IsWithinCityLimits,
            occupancyType: deliveryAddress?.OccupancyType);

        var newLine = InsuranceLine.Create(
            packageId: package.Id,
            salePrice: request.PremiumAmount,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Buyer,
            shouldExcludeFromPricing: false,
            details: details);

        package.AddLine(newLine);
    }
}
