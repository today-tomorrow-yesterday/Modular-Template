using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Tax.GetTaxQuestions;

public sealed record GetTaxQuestionsQuery(
    string StateCode,
    int HomeTypeId) : IQuery<IReadOnlyCollection<TaxQuestionResult>>;

public sealed record TaxQuestionResult(
    int QuestionNumber,
    string Text);
