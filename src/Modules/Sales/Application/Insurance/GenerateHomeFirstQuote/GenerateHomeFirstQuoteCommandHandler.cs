using Modules.Sales.Domain;
using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Insurance;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Insurance.GenerateHomeFirstQuote;

// Flow: POST /api/v1/sales/{saleId}/insurance/quote?type=home-first → GenerateHomeFirstQuoteCommand →
//   occupancy eligibility check → iSeries CalculateHomeFirstQuote →
//   upsert InsuranceLine → recalculate GrossProfit → SaveChangesAsync
internal sealed class GenerateHomeFirstQuoteCommandHandler(
    ISaleRepository saleRepository,
    IiSeriesAdapter iSeriesAdapter,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<GenerateHomeFirstQuoteCommand, HomeFirstQuoteResult>
{

    public async Task<Result<HomeFirstQuoteResult>> Handle(
        GenerateHomeFirstQuoteCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale with full context (packages, delivery address, retail location, party)
        var sale = await saleRepository.GetByPublicIdWithFullContextAsync(
            request.SalePublicId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<HomeFirstQuoteResult>(
                SaleErrors.NotFoundByPublicId(request.SalePublicId));
        }

        // Find primary package and home line
        var primaryPackage = sale.Packages.FirstOrDefault(p => p.IsPrimaryPackage);
        if (primaryPackage is null)
        {
            return Result.Failure<HomeFirstQuoteResult>(Error.Validation(
                "Insurance.NoPrimaryPackage", "Cannot generate HomeFirst quote without a primary package."));
        }

        var homeLine = primaryPackage.Lines.OfType<HomeLine>().SingleOrDefault();
        if (homeLine?.Details is null)
        {
            return Result.Failure<HomeFirstQuoteResult>(Error.Validation(
                "Insurance.NoHomeLine", "Cannot generate HomeFirst quote without a home line on the primary package."));
        }

        // Step 2: Occupancy eligibility check (from delivery address, not request)
        var deliveryAddress = sale.DeliveryAddress;
        if (DeliveryAddress.IsOccupancyInsuranceIneligible(deliveryAddress?.OccupancyType))
        {
            // Remove HomeFirst Insurance line only — legacy did NOT strip warranty on occupancy ineligibility
            var insuranceLine = primaryPackage.Lines.OfType<InsuranceLine>()
                .SingleOrDefault(l => l.Details?.InsuranceType == InsuranceType.HomeFirst);
            if (insuranceLine is not null)
            {
                primaryPackage.RemoveLine(insuranceLine);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<HomeFirstQuoteResult>(Error.Validation(
                "Insurance.IneligibleOccupancy",
                $"Occupancy type '{deliveryAddress!.OccupancyType}' is not eligible for HomeFirst insurance."));
        }

        var homeDetails = homeLine.Details;
        var homeCenterNumber = sale.RetailLocation.RefHomeCenterNumber ?? 0;

        // Step 3: Map occupancy type from request char for iSeries call
        var occupancyType = request.OccupancyType switch
        {
            'P' or 'p' => OccupancyType.Primary,
            'S' or 's' => OccupancyType.Secondary,
            'R' or 'r' => OccupancyType.Rental,
            _ => OccupancyType.Primary
        };

        // Get customer names from party cache
        var party = sale.Party;
        var firstName = party.Person?.FirstName ?? string.Empty;
        var lastName = party.Person?.LastName ?? string.Empty;
        var phone = party.Person?.Phone;

        // Build HomeFirst quote request
        var quoteRequest = new HomeFirstQuoteRequest
        {
            HomeCenterNumber = homeCenterNumber,
            StockNumber = homeDetails.StockNumber ?? string.Empty,
            ModelNumber = homeDetails.Model ?? string.Empty,
            CoverageAmount = request.CoverageAmount,
            ModelYear = homeDetails.ModelYear ?? DateTime.UtcNow.Year,
            DeliveryZipCode = deliveryAddress?.PostalCode ?? string.Empty,
            HomeCondition = MapHomeCondition(homeDetails.HomeType),
            SerialNumber = homeDetails.SerialNumbers?.FirstOrDefault() ?? string.Empty,
            LengthInFeet = (int)(homeDetails.LengthInFeet ?? 0),
            WidthInFeet = (int)(homeDetails.WidthInFeet ?? 0),
            OccupancyType = occupancyType,
            InParkOrSubdivision = request.IsHomeLocatedInPark,
            HasFoundationOrMasonry = request.IsHomeOnPermanentFoundation,
            IsLandOwnedByCustomer = request.IsLandCustomerOwned,
            FirstName = firstName,
            LastName = lastName,
            MailingAddress = request.MailingAddress,
            MailingCity = request.MailingCity,
            MailingState = request.MailingState,
            MailingZip = request.MailingZip,
            LocationAddress = deliveryAddress?.AddressLine1 ?? string.Empty,
            LocationCity = deliveryAddress?.City ?? string.Empty,
            LocationState = deliveryAddress?.State ?? string.Empty,
            IsWithinCityLimits = deliveryAddress?.IsWithinCityLimits ?? false,
            BuyerBirthDate = DateOnly.FromDateTime(request.CustomerBirthDate),
            CoBuyerBirthDate = request.CoApplicantBirthDate.HasValue
                ? DateOnly.FromDateTime(request.CoApplicantBirthDate.Value)
                : null,
            PhoneNumber = phone
        };

        // Call iSeries adapter
        var quoteResult = await iSeriesAdapter.CalculateHomeFirstQuote(quoteRequest, cancellationToken);

        // Check for iSeries error
        var isEligible = string.IsNullOrEmpty(quoteResult.ErrorMessage);
        if (!isEligible)
        {
            return new HomeFirstQuoteResult(
                quoteResult.TempLinkId,
                quoteResult.InsuranceCompanyName,
                quoteResult.TotalPremium,
                request.CoverageAmount,
                quoteResult.MaximumCoverage,
                false,
                quoteResult.ErrorMessage);
        }

        // Step 4: Upsert Insurance line (PUT semantics — delete old, insert new)
        var existingInsurance = primaryPackage.Lines.OfType<InsuranceLine>()
            .SingleOrDefault(l => l.Details?.InsuranceType == InsuranceType.HomeFirst);
        if (existingInsurance is not null)
        {
            primaryPackage.RemoveLine(existingInsurance);
        }

        var details = InsuranceDetails.Create(
            insuranceType: InsuranceType.HomeFirst,
            coverageAmount: request.CoverageAmount,
            hasFoundationOrMasonry: request.IsHomeOnPermanentFoundation,
            inParkOrSubdivision: request.IsHomeLocatedInPark,
            isLandOwnedByCustomer: request.IsLandCustomerOwned,
            isPremiumFinanced: request.IsPremiumFinanced,
            companyName: quoteResult.InsuranceCompanyName,
            maxCoverage: quoteResult.MaximumCoverage,
            totalPremium: quoteResult.TotalPremium,
            tempLinkId: quoteResult.TempLinkId);

        var newLine = InsuranceLine.Create(
            packageId: primaryPackage.Id,
            salePrice: quoteResult.TotalPremium,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Buyer,
            shouldExcludeFromPricing: false,
            details: details);

        primaryPackage.AddLine(newLine);

        // Step 5: GrossProfit recalculated automatically by Package.AddLine/RemoveLine
        // Step 6: Save
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new HomeFirstQuoteResult(
            quoteResult.TempLinkId,
            quoteResult.InsuranceCompanyName,
            quoteResult.TotalPremium,
            request.CoverageAmount,
            quoteResult.MaximumCoverage,
            true,
            null);
    }

    private static HomeCondition MapHomeCondition(HomeType homeType) => homeType switch
    {
        HomeType.New => HomeCondition.New,
        HomeType.Used => HomeCondition.Used,
        HomeType.Repo => HomeCondition.Repo,
        _ => HomeCondition.New
    };
}
