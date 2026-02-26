using Modules.Sales.Domain;
using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;
using ISeriesInsurance = Rtl.Core.Application.Adapters.ISeries.Insurance;

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
        // Step 1: Load sale with full context and validate prerequisites
        var contextResult = await LoadAndValidateSaleContextAsync(request.SalePublicId, cancellationToken);
        if (contextResult.IsFailure)
        {
            return Result.Failure<HomeFirstQuoteResult>(contextResult.Error);
        }

        var ctx = contextResult.Value;

        // Step 2: Occupancy eligibility check — strip line and fail if ineligible
        if (DeliveryAddress.IsOccupancyInsuranceIneligible(ctx.DeliveryAddress?.OccupancyType))
        {
            return await RejectOccupancyIneligibleAsync(ctx, cancellationToken);
        }

        // Step 3: Call iSeries adapter to calculate HomeFirst quote
        var occupancyType = MapOccupancyType(request.OccupancyType);
        var quoteResult = await CalculateHomeFirstQuoteAsync(request, ctx, occupancyType, cancellationToken);

        // Step 4: Return partial result if iSeries rejected the quote
        if (!string.IsNullOrEmpty(quoteResult.ErrorMessage))
        {
            return new HomeFirstQuoteResult(
                quoteResult.TempLinkId, quoteResult.InsuranceCompanyName,
                quoteResult.TotalPremium, request.CoverageAmount,
                quoteResult.MaximumCoverage, false, quoteResult.ErrorMessage);
        }

        // Step 5: Upsert Insurance line with new quote (PUT semantics)
        UpsertInsuranceLine(ctx, request, quoteResult, occupancyType);

        // Step 6: Persist changes
        ctx.PrimaryPackage.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 7: Return quote result
        return new HomeFirstQuoteResult(
            quoteResult.TempLinkId, quoteResult.InsuranceCompanyName,
            quoteResult.TotalPremium, request.CoverageAmount,
            quoteResult.MaximumCoverage, true, null);
    }

    private async Task<Result<ValidatedSaleContext>> LoadAndValidateSaleContextAsync(
        Guid salePublicId, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdWithFullContextAsync(salePublicId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<ValidatedSaleContext>(SaleErrors.NotFoundByPublicId(salePublicId));
        }

        var primaryPackage = sale.Packages.FirstOrDefault(p => p.IsPrimaryPackage);
        if (primaryPackage is null)
        {
            return Result.Failure<ValidatedSaleContext>(Error.Validation(
                "Insurance.NoPrimaryPackage", "Cannot generate HomeFirst quote without a primary package."));
        }

        var homeLine = primaryPackage.Lines.OfType<HomeLine>().SingleOrDefault();
        if (homeLine?.Details is null)
        {
            return Result.Failure<ValidatedSaleContext>(Error.Validation(
                "Insurance.NoHomeLine", "Cannot generate HomeFirst quote without a home line on the primary package."));
        }

        return new ValidatedSaleContext(sale, primaryPackage, homeLine.Details, sale.DeliveryAddress);
    }

    private async Task<Result<HomeFirstQuoteResult>> RejectOccupancyIneligibleAsync(
        ValidatedSaleContext ctx, CancellationToken cancellationToken)
    {
        // Remove HomeFirst Insurance line only — legacy did NOT strip warranty on occupancy ineligibility
        ctx.PrimaryPackage.RemoveHomeFirstInsuranceLine();

        ctx.PrimaryPackage.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Failure<HomeFirstQuoteResult>(Error.Validation(
            "Insurance.IneligibleOccupancy",
            $"Occupancy type '{ctx.DeliveryAddress!.OccupancyType}' is not eligible for HomeFirst insurance."));
    }

    private async Task<ISeriesInsurance.HomeFirstQuoteResult> CalculateHomeFirstQuoteAsync(
        GenerateHomeFirstQuoteCommand request, ValidatedSaleContext ctx,
        OccupancyType occupancyType, CancellationToken cancellationToken)
    {
        var homeDetails = ctx.HomeDetails;
        var deliveryAddress = ctx.DeliveryAddress;
        var party = ctx.Sale.Party;

        var quoteRequest = new ISeriesInsurance.HomeFirstQuoteRequest
        {
            HomeCenterNumber = ctx.Sale.RetailLocation.RefHomeCenterNumber ?? 0,
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
            FirstName = party.Person?.FirstName ?? string.Empty,
            LastName = party.Person?.LastName ?? string.Empty,
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
            PhoneNumber = party.Person?.Phone
        };

        return await iSeriesAdapter.CalculateHomeFirstQuote(quoteRequest, cancellationToken);
    }

    private static void UpsertInsuranceLine(
        ValidatedSaleContext ctx, GenerateHomeFirstQuoteCommand request,
        ISeriesInsurance.HomeFirstQuoteResult quoteResult, OccupancyType occupancyType)
    {
        ctx.PrimaryPackage.RemoveHomeFirstInsuranceLine();

        var homeDetails = ctx.HomeDetails;
        var deliveryAddress = ctx.DeliveryAddress;

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
            tempLinkId: quoteResult.TempLinkId,
            homeStockNumber: homeDetails.StockNumber,
            homeModelYear: homeDetails.ModelYear,
            homeLengthInFeet: homeDetails.LengthInFeet,
            homeWidthInFeet: homeDetails.WidthInFeet,
            homeCondition: homeDetails.HomeType.ToString(),
            deliveryState: deliveryAddress?.State,
            deliveryPostalCode: deliveryAddress?.PostalCode,
            deliveryCity: deliveryAddress?.City,
            deliveryIsWithinCityLimits: deliveryAddress?.IsWithinCityLimits,
            occupancyType: occupancyType.ToString());

        ctx.PrimaryPackage.AddLine(InsuranceLine.Create(
            packageId: ctx.PrimaryPackage.Id,
            salePrice: quoteResult.TotalPremium,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            responsibility: Responsibility.Buyer,
            shouldExcludeFromPricing: false,
            details: details));
    }

    private static OccupancyType MapOccupancyType(char occupancyType) => occupancyType switch
    {
        'P' or 'p' => OccupancyType.Primary,
        'S' or 's' => OccupancyType.Secondary,
        'R' or 'r' => OccupancyType.Rental,
        _ => OccupancyType.Primary
    };

    private static HomeCondition MapHomeCondition(HomeType homeType) => homeType switch
    {
        HomeType.New => HomeCondition.New,
        HomeType.Used => HomeCondition.Used,
        HomeType.Repo => HomeCondition.Repo,
        _ => HomeCondition.New
    };

    private sealed record ValidatedSaleContext(
        Sale Sale, Package PrimaryPackage, HomeDetails HomeDetails, DeliveryAddress? DeliveryAddress);
}
