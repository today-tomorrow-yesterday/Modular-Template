using Modules.Sales.Domain;
using Modules.Sales.Domain.Cdc;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.ProjectCosts;
using Modules.Sales.Domain.Packages.Tax;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Tax.UpdatePackageTax;

// Flow: PUT /api/v1/packages/{packageId}/tax → UpdatePackageTaxCommand →
//   enrich QuestionText from CDC → upsert Tax line (config only, no calculation) →
//   clear prior calculation → remove Use Tax PC → set MustRecalculateTaxes = true →
//   recalculate GrossProfit.
internal sealed class UpdatePackageTaxCommandHandler(
    IPackageRepository packageRepository,
    ICdcTaxQueries cdcTaxQueries,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpdatePackageTaxCommand, UpdatePackageTaxResult>
{

    public async Task<Result<UpdatePackageTaxResult>> Handle(
        UpdatePackageTaxCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load package with all lines
        var package = await packageRepository.GetByPublicIdWithSaleContextAsync(
            request.PackagePublicId, cancellationToken);

        if (package is null)
        {
            return Result.Failure<UpdatePackageTaxResult>(
                PackageErrors.NotFoundByPublicId(request.PackagePublicId));
        }

        // Step 2: Enrich QuestionText from cdc.tax_question_text
        var questionNumbers = request.QuestionAnswers.Select(q => q.QuestionNumber).ToList();
        var questionTexts = await cdcTaxQueries.GetQuestionTextsByNumbersAsync(questionNumbers, cancellationToken);

        var enrichedAnswers = request.QuestionAnswers
            .Select(q => TaxQuestionAnswer.Create(
                q.QuestionNumber,
                q.Answer ? "Y" : "N",
                questionTexts.GetValueOrDefault(q.QuestionNumber)))
            .ToList();

        // Step 2b: Look up exemption description from CDC when TaxExemptionId is set
        string? taxExemptionDescription = null;
        if (request.TaxExemptionId is not null and not 0)
        {
            var exemptions = await cdcTaxQueries.GetActiveExemptionsAsync(cancellationToken);
            taxExemptionDescription = exemptions
                .FirstOrDefault(e => e.Id == request.TaxExemptionId)?.Description;
        }

        // Step 3: Upsert Tax line (PUT semantics — delete old, insert new)
        package.RemoveLine<TaxLine>();

        var deliveryAddress = package.Sale?.DeliveryAddress;

        var taxDetails = TaxDetails.Create(
            previouslyTitled: request.PreviouslyTitled,
            taxExemptionId: request.TaxExemptionId,
            questionAnswers: enrichedAnswers,
            taxes: [],
            errors: null,
            taxExemptionDescription: taxExemptionDescription,
            stateCode: package.Sale?.RetailLocation?.StateCode,
            deliveryCity: deliveryAddress?.City,
            deliveryCounty: deliveryAddress?.County,
            deliveryPostalCode: deliveryAddress?.PostalCode,
            deliveryIsWithinCityLimits: deliveryAddress?.IsWithinCityLimits);

        var newTaxLine = TaxLine.Create(
            packageId: package.Id,
            salePrice: 0m,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            shouldExcludeFromPricing: false,
            details: taxDetails);

        package.AddLine(newTaxLine);

        // Step 4: Remove Use Tax ProjectCost (Cat 9, Item 21) if present
        package.RemoveProjectCost(ProjectCostCategories.UseTax, ProjectCostItems.UseTax);

        // Step 5: Tax config changed — set MustRecalculateTaxes = true
        package.FlagForTaxRecalculation();

        // Step 6: Persist
        package.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageTaxResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

}
