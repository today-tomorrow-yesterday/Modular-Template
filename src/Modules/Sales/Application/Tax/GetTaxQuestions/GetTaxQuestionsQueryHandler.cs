using Modules.Sales.Domain.Cdc;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Tax.GetTaxQuestions;

internal sealed class GetTaxQuestionsQueryHandler(ICdcTaxQueries cdcTaxQueries)
    : IQueryHandler<GetTaxQuestionsQuery, IReadOnlyCollection<TaxQuestionResult>>
{
    public async Task<Result<IReadOnlyCollection<TaxQuestionResult>>> Handle(
        GetTaxQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var questions = await cdcTaxQueries.GetQuestionsForStateAndHomeTypeAsync(
            request.StateCode,
            request.HomeTypeId,
            cancellationToken);

        var results = questions.Select(q => new TaxQuestionResult(
            q.QuestionNumber,
            q.Text)).ToList();

        return results;
    }
}
