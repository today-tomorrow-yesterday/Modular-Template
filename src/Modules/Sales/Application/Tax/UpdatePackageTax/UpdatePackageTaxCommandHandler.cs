using Modules.Sales.Domain;
using Modules.Sales.Domain.Cdc;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Details;
using Modules.Sales.Domain.Packages.Lines;
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
    private const int UseTaxCategoryNumber = 9;
    private const int UseTaxItemNumber = 21;

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

        // Step 3: Upsert Tax line (PUT semantics — delete old, insert new)
        package.RemoveTaxLine();

        var taxDetails = TaxDetails.Create(
            previouslyTitled: request.PreviouslyTitled,
            taxExemptionId: request.TaxExemptionId,
            questionAnswers: enrichedAnswers,
            taxes: [],
            errors: null);

        var newTaxLine = TaxLine.Create(
            packageId: package.Id,
            salePrice: 0m,
            estimatedCost: 0m,
            retailSalePrice: 0m,
            shouldExcludeFromPricing: false,
            details: taxDetails);

        package.AddLine(newTaxLine);

        // Step 4: Remove Use Tax ProjectCost (Cat 9, Item 21) if present
        package.RemoveProjectCost(UseTaxCategoryNumber, UseTaxItemNumber);

        // Step 5: Tax config changed — set MustRecalculateTaxes = true
        package.FlagForTaxRecalculation();

        // Step 6: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdatePackageTaxResult(
            package.GrossProfit,
            package.CommissionableGrossProfit,
            package.MustRecalculateTaxes);
    }

}
