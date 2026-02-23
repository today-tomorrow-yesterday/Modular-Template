namespace Modules.Sales.Domain.Cdc;

public interface ICdcTaxQueries
{
    Task<IReadOnlyCollection<CdcTaxExemption>> GetActiveExemptionsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TaxQuestionWithText>> GetQuestionsForStateAndHomeTypeAsync(
        string stateCode,
        int homeTypeId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<int, string>> GetQuestionTextsByNumbersAsync(
        IReadOnlyCollection<int> questionNumbers,
        CancellationToken cancellationToken = default);
}

// Flattened projection: question + its active text
public sealed record TaxQuestionWithText(int QuestionNumber, string Text);
