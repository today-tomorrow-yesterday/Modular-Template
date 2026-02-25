using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Lines;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.DeliveryAddresses.UpdateDeliveryAddress;

// Flow: Sales.UpdateDeliveryAddressCommand → update Sales.delivery_addresses → raises Sales.DeliveryAddressChanged
// Inlined side-effects: state change clears tax Q&A, occupancy ineligibility removes insurance/warranty,
// location change clears tax calculations and removes Use Tax project cost.
internal sealed class UpdateDeliveryAddressCommandHandler(
    ISaleRepository saleRepository,
    IPackageRepository packageRepository,
    IUnitOfWork<Domain.ISalesModule> unitOfWork)
    : ICommandHandler<UpdateDeliveryAddressCommand>
{
    // Use Tax is a project cost with Category 9, Item 21 in iSeries.
    internal const int UseTaxCategoryNumber = 9;
    internal const int UseTaxItemNumber = 21;

    public async Task<Result> Handle(
        UpdateDeliveryAddressCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale and validate delivery address exists
        var addressResult = await LoadExistingDeliveryAddressAsync(request.SalePublicId, cancellationToken);
        if (addressResult.IsFailure)
            return Result.Failure(addressResult.Error);

        var deliveryAddress = addressResult.Value;

        // Step 2: Apply address updates — returns what changed
        var changes = ApplyAddressUpdates(deliveryAddress, request);

        // Step 3: Apply package side-effects if any relevant fields changed
        if (changes.StateChanged || changes.OccupancyBecameIneligible || changes.LocationChanged)
        {
            var packages = await packageRepository
                .GetBySaleIdWithTrackingAsync(deliveryAddress.SaleId, cancellationToken);

            foreach (var package in packages)
            {
                if (changes.OccupancyBecameIneligible)
                {
                    // Legacy cascades to ALL packages (not just Draft) — occupancy ineligibility
                    // overrides regardless of package status.
                    package.RemoveHomeFirstInsuranceLine();
                    package.RemoveWarrantyLine();
                }

                if (changes.StateChanged)
                {
                    var taxLine = package.Lines.OfType<TaxLine>().FirstOrDefault();
                    taxLine?.ClearQuestionAnswers();
                    package.FlagForTaxRecalculation();
                }

                if (changes.LocationChanged && package.Status == PackageStatus.Draft)
                {
                    var taxLine = package.Lines.OfType<TaxLine>().FirstOrDefault();
                    taxLine?.ClearCalculations();
                    package.RemoveProjectCost(UseTaxCategoryNumber, UseTaxItemNumber);
                    package.FlagForTaxRecalculation();
                }
            }
        }

        // Step 4: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<DeliveryAddress>> LoadExistingDeliveryAddressAsync(
        Guid salePublicId, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdWithDeliveryAddressAsync(
            salePublicId, cancellationToken);

        if (sale is null)
            return Result.Failure<DeliveryAddress>(SaleErrors.NotFoundByPublicId(salePublicId));

        if (sale.DeliveryAddress is null)
            return Result.Failure<DeliveryAddress>(DeliveryAddressErrors.NotFound(sale.Id));

        return sale.DeliveryAddress;
    }

    private static DeliveryAddressChangeResult ApplyAddressUpdates(
        DeliveryAddress deliveryAddress, UpdateDeliveryAddressCommand request)
    {
        return deliveryAddress.Update(
            request.OccupancyType,
            request.IsWithinCityLimits,
            addressStyle: null,
            addressType: null,
            request.AddressLine1,
            addressLine2: null,
            addressLine3: null,
            request.City,
            request.County,
            NormalizeStateAbbreviation(request.State),
            country: null,
            request.PostalCode);
    }

    // Validator enforces MaxLength(2) — full state names never reach the handler.
    // Just normalize casing for iSeries compatibility.
    private static string? NormalizeStateAbbreviation(string? state) =>
        string.IsNullOrWhiteSpace(state) ? state : state.ToUpperInvariant();
}
