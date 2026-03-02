using Microsoft.Extensions.Logging;
using Modules.Sales.Domain;
using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Events;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.Packages.Warranty;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using ISeriesInsurance = Rtl.Core.Application.Adapters.ISeries.Insurance;

namespace Modules.Sales.Application.Packages.EventHandlers;

// Flow: HomeLineUpdatedDomainEvent → occupancy eligibility check → HomeFirst recalc (best-effort)
// Raised when a HomeLine is added to a package via Package.AddLine().
internal sealed class HomeLineUpdatedDomainEventHandler(
    IPackageRepository packageRepository,
    IiSeriesAdapter iSeriesAdapter,
    IUnitOfWork<ISalesModule> unitOfWork,
    ILogger<HomeLineUpdatedDomainEventHandler> logger)
    : DomainEventHandler<HomeLineUpdatedDomainEvent>
{
    public override async Task Handle(
        HomeLineUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var package = await packageRepository.GetByIdWithSaleContextAsync(
            domainEvent.PackageId, cancellationToken);

        if (package is null)
            return;

        var deliveryAddress = package.Sale.DeliveryAddress;
        var homeLine = package.Lines.OfType<HomeLine>().SingleOrDefault();
        if (homeLine is null)
            return;

        // 1. Occupancy eligibility enforcement
        if (deliveryAddress is not null &&
            DeliveryAddress.IsOccupancyInsuranceIneligible(deliveryAddress.OccupancyType))
        {
            package.RemoveHomeFirstInsuranceLine();
            package.RemoveOutsideInsuranceLine();
            package.RemoveLine<WarrantyLine>();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return; // No point recalculating insurance we just removed
        }

        // 2. HomeFirst recalc (if HomeFirst insurance line exists)
        var homeFirstLine = package.Lines.OfType<InsuranceLine>()
            .SingleOrDefault(l => l.Details?.InsuranceType == InsuranceType.HomeFirst);

        if (homeFirstLine?.Details is null)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        try
        {
            var quoteRequest = BuildQuoteRequest(homeLine, deliveryAddress, homeFirstLine.Details, package);
            var quoteResult = await iSeriesAdapter.CalculateHomeFirstQuote(quoteRequest, cancellationToken);

            if (string.IsNullOrEmpty(quoteResult.ErrorMessage))
            {
                homeFirstLine.UpdatePricing(quoteResult.TotalPremium, 0m, quoteResult.TotalPremium);
                package.RecalculateGrossProfit();
            }
            else
            {
                logger.LogWarning(
                    "HomeFirst recalc returned error for Package {PackageId}: {Error}",
                    domainEvent.PackageId, quoteResult.ErrorMessage);
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex,
                "HomeFirst recalc unavailable for Package {PackageId} — keeping existing premium",
                domainEvent.PackageId);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex,
                "HomeFirst recalc timed out for Package {PackageId} — keeping existing premium",
                domainEvent.PackageId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ISeriesInsurance.HomeFirstQuoteRequest BuildQuoteRequest(
        HomeLine homeLine,
        DeliveryAddress? deliveryAddress,
        InsuranceDetails insuranceDetails,
        Package package)
    {
        var homeDetails = homeLine.Details;

        return new ISeriesInsurance.HomeFirstQuoteRequest
        {
            HomeCenterNumber = package.Sale.RetailLocation?.RefHomeCenterNumber ?? 0,
            StockNumber = homeDetails?.StockNumber ?? string.Empty,
            ModelNumber = homeDetails?.Model ?? string.Empty,
            CoverageAmount = insuranceDetails.CoverageAmount,
            ModelYear = homeDetails?.ModelYear ?? DateTime.UtcNow.Year,
            DeliveryZipCode = deliveryAddress?.PostalCode ?? string.Empty,
            HomeCondition = MapHomeCondition(homeDetails?.HomeType ?? HomeType.New),
            SerialNumber = homeDetails?.SerialNumbers?.FirstOrDefault() ?? string.Empty,
            LengthInFeet = (int)(homeDetails?.LengthInFeet ?? 0),
            WidthInFeet = (int)(homeDetails?.WidthInFeet ?? 0),
            OccupancyType = MapOccupancyType(insuranceDetails.OccupancyType),
            InParkOrSubdivision = insuranceDetails.InParkOrSubdivision,
            HasFoundationOrMasonry = insuranceDetails.HasFoundationOrMasonry,
            IsLandOwnedByCustomer = insuranceDetails.IsLandOwnedByCustomer,
            FirstName = package.Sale.Party?.Person?.FirstName ?? string.Empty,
            LastName = package.Sale.Party?.Person?.LastName ?? string.Empty,
            LocationAddress = deliveryAddress?.AddressLine1 ?? string.Empty,
            LocationCity = deliveryAddress?.City ?? string.Empty,
            LocationState = deliveryAddress?.State ?? string.Empty,
            IsWithinCityLimits = deliveryAddress?.IsWithinCityLimits ?? false,
            PhoneNumber = package.Sale.Party?.Person?.Phone
        };
    }

    private static OccupancyType MapOccupancyType(string? occupancyType) => occupancyType switch
    {
        "Primary" => OccupancyType.Primary,
        "Secondary" => OccupancyType.Secondary,
        "Rental" => OccupancyType.Rental,
        _ => OccupancyType.Primary
    };

    private static HomeCondition MapHomeCondition(HomeType homeType) => homeType switch
    {
        HomeType.New => HomeCondition.New,
        HomeType.Used => HomeCondition.Used,
        HomeType.Repo => HomeCondition.Repo,
        _ => HomeCondition.New
    };
}
