using Modules.Sales.Domain;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Insurance;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Insurance.GenerateWarrantyQuote;

// Flow: POST /sales/{saleId}/insurance/quote?type=warranty → GenerateWarrantyQuoteCommand →
//   derive 8 inputs from sale context → iSeries CalculateWarrantyQuote →
//   upsert WarrantyLine + tax change detection → return quote result
internal sealed class GenerateWarrantyQuoteCommandHandler(
    ISaleRepository saleRepository,
    IiSeriesAdapter iSeriesAdapter,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<GenerateWarrantyQuoteCommand, GenerateWarrantyQuoteResult>
{
    public async Task<Result<GenerateWarrantyQuoteResult>> Handle(
        GenerateWarrantyQuoteCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale with full context and validate prerequisites
        var contextResult = await LoadAndValidateSaleContextAsync(request.SalePublicId, cancellationToken);
        if (contextResult.IsFailure)
        {
            return Result.Failure<GenerateWarrantyQuoteResult>(contextResult.Error);
        }

        var (sale, primaryPackage, homeDetails) = contextResult.Value;

        // Step 2: Snapshot existing warranty state for tax change detection
        var previousState = CaptureExistingWarrantyState(primaryPackage);

        // Step 3: Call iSeries adapter to calculate warranty quote
        var quoteResult = await CalculateWarrantyQuoteAsync(sale, homeDetails, cancellationToken);

        // Step 4: Replace existing warranty line with new quote (PUT semantics)
        UpsertWarrantyLine(primaryPackage, quoteResult, sale, homeDetails);

        // Step 5: Flag for tax recalculation if warranty amount or selection changed
        FlagTaxRecalculationIfChanged(primaryPackage, previousState, quoteResult);

        // Step 6: Persist changes
        primaryPackage.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 7: Return quote result
        return new GenerateWarrantyQuoteResult(
            quoteResult.Premium,
            quoteResult.SalesTaxPremium,
            WarrantySelected: true);
    }

    private async Task<Result<ValidatedSaleContext>> LoadAndValidateSaleContextAsync(
        Guid salePublicId, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdWithFullContextAsync(salePublicId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<ValidatedSaleContext>(SaleErrors.NotFoundByPublicId(salePublicId));
        }

        if (sale.DeliveryAddress is null)
        {
            return Result.Failure<ValidatedSaleContext>(WarrantyErrors.NoDeliveryAddress());
        }

        // No occupancy guard for warranty quotes — legacy had no such restriction.
        // Occupancy eligibility is only checked for HomeFirst insurance quotes.

        var primaryPackage = sale.Packages.FirstOrDefault(p => p.IsPrimaryPackage);
        if (primaryPackage is null)
        {
            return Result.Failure<ValidatedSaleContext>(WarrantyErrors.NoPrimaryPackage());
        }

        var homeLine = primaryPackage.Lines.OfType<HomeLine>().SingleOrDefault();
        if (homeLine?.Details is null)
        {
            return Result.Failure<ValidatedSaleContext>(WarrantyErrors.NoHomeLine());
        }

        if (homeLine.Details.ModelYear is null || homeLine.Details.ModularType is null)
        {
            return Result.Failure<ValidatedSaleContext>(WarrantyErrors.MissingHomeDetails());
        }

        return new ValidatedSaleContext(sale, primaryPackage, homeLine.Details);
    }

    private static PreviousWarrantyState CaptureExistingWarrantyState(Package primaryPackage)
    {
        var existingLine = primaryPackage.Lines.OfType<WarrantyLine>().SingleOrDefault();
        return new PreviousWarrantyState(
            existingLine,
            existingLine?.Details?.WarrantyAmount,
            existingLine?.Details?.WarrantySelected ?? false);
    }

    private async Task<WarrantyQuoteResult> CalculateWarrantyQuoteAsync(
        Sale sale, HomeDetails homeDetails, CancellationToken cancellationToken)
    {
        var request = new WarrantyQuoteRequest
        {
            HomeCenterNumber = sale.RetailLocation.RefHomeCenterNumber ?? 0,
            AppId = 0, // Legacy hardcodes 0 — warranty quote is stateless (no iSeries application link)
            PhysicalState = sale.DeliveryAddress!.State ?? string.Empty,
            PhysicalZip = sale.DeliveryAddress.PostalCode ?? string.Empty,
            WidthInFeet = (int)(homeDetails.WidthInFeet ?? 0),
            ModelYear = homeDetails.ModelYear!.Value,
            HomeCondition = MapHomeCondition(homeDetails.HomeType),
            ModularClassification = MapModularClassification(homeDetails.ModularType!.Value),
            IsWithinCityLimits = sale.DeliveryAddress.IsWithinCityLimits,
            CalculateSalesTax = true
        };
        return await iSeriesAdapter.CalculateWarrantyQuote(request, cancellationToken);
    }

    private static void UpsertWarrantyLine(
        Package primaryPackage, WarrantyQuoteResult quoteResult,
        Sale sale, HomeDetails homeDetails)
    {
        primaryPackage.RemoveLine<WarrantyLine>();

        var warrantyDetails = WarrantyDetails.Create(
            quoteResult.Premium,
            quoteResult.SalesTaxPremium,
            homeModelYear: homeDetails.ModelYear,
            homeModularType: homeDetails.ModularType?.ToString(),
            homeWidthInFeet: homeDetails.WidthInFeet,
            homeCondition: homeDetails.HomeType.ToString(),
            deliveryState: sale.DeliveryAddress?.State,
            deliveryPostalCode: sale.DeliveryAddress?.PostalCode,
            deliveryIsWithinCityLimits: sale.DeliveryAddress?.IsWithinCityLimits,
            homeCenterNumber: sale.RetailLocation.RefHomeCenterNumber);

        primaryPackage.AddLine(WarrantyLine.Create(
            primaryPackage.Id,
            salePrice: quoteResult.Premium,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            shouldExcludeFromPricing: false,
            details: warrantyDetails));
    }

    private static void FlagTaxRecalculationIfChanged(
        Package primaryPackage, PreviousWarrantyState previousState, WarrantyQuoteResult quoteResult)
    {
        if (previousState.Amount != quoteResult.Premium || !previousState.WasSelected)
        {
            var taxLine = primaryPackage.Lines.OfType<TaxLine>().SingleOrDefault();
            taxLine?.ClearCalculations();

            primaryPackage.RemoveProjectCost(ProjectCostCategories.UseTax, ProjectCostItems.UseTax);

            primaryPackage.FlagForTaxRecalculation();
        }
    }

    private static HomeCondition MapHomeCondition(HomeType homeType) => homeType switch
    {
        HomeType.New => HomeCondition.New,
        HomeType.Used => HomeCondition.Used,
        HomeType.Repo => HomeCondition.Repo,
        _ => HomeCondition.New
    };

    private static ModularClassification MapModularClassification(ModularType modularType) => modularType switch
    {
        ModularType.Hud => ModularClassification.Hud,
        ModularType.OnFrame => ModularClassification.OnFrame,
        ModularType.Mod => ModularClassification.OffFrame,
        ModularType.OffFrame => ModularClassification.OffFrame,
        _ => ModularClassification.Hud
    };

    private sealed record ValidatedSaleContext(Sale Sale, Package PrimaryPackage, HomeDetails HomeDetails);

    private sealed record PreviousWarrantyState(WarrantyLine? ExistingLine, decimal? Amount, bool WasSelected);
}
