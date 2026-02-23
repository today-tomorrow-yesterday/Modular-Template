using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class CdcTaxQueries(SalesDbContext dbContext) : ICdcTaxQueries
{
    private const int MasterDealer = 29;

    public async Task<IReadOnlyCollection<CdcTaxExemption>> GetActiveExemptionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CdcTaxExemptions
            .Where(e => e.IsActive)
            .OrderBy(e => e.ExemptionCode)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TaxQuestionWithText>> GetQuestionsForStateAndHomeTypeAsync(
        string stateCode,
        int homeTypeId,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Filter questions by master_dealer, state, date range, and home type flag
        var questions = dbContext.CdcTaxQuestions
            .Where(q => q.MasterDealer == MasterDealer)
            .Where(q => q.StateCode == stateCode)
            .Where(q => q.EffectiveDate <= today)
            .Where(q => q.EndDate == null || q.EndDate >= today)
            .Where(q =>
                (homeTypeId == 0 && q.AskForNew) ||    // HomeType.New
                (homeTypeId == 1 && q.AskForUsed) ||   // HomeType.Used
                (homeTypeId == 2 && q.AskForRepo) ||   // HomeType.Repo
                (homeTypeId == 3 && q.AskForLand));    // Land

        // Join to active question texts
        var result = await questions
            .SelectMany(q => q.QuestionTexts
                .Where(t => t.IsActive)
                .Select(t => new TaxQuestionWithText(q.QuestionNumber, t.Text)))
            .OrderBy(r => r.QuestionNumber)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<Dictionary<int, string>> GetQuestionTextsByNumbersAsync(
        IReadOnlyCollection<int> questionNumbers,
        CancellationToken cancellationToken = default)
    {
        if (questionNumbers.Count == 0)
            return new Dictionary<int, string>();

        return await dbContext.CdcTaxQuestionTexts
            .Where(t => questionNumbers.Contains(t.QuestionNumber) && t.IsActive)
            .AsNoTracking()
            .ToDictionaryAsync(t => t.QuestionNumber, t => t.Text, cancellationToken);
    }
}
