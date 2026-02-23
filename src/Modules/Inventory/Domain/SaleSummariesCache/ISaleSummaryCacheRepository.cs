using Rtl.Core.Domain;

namespace Modules.Inventory.Domain.SaleSummariesCache;

public interface ISaleSummaryCacheRepository : IReadRepository<SaleSummaryCache, int>
{
    Task<IReadOnlyCollection<SaleSummaryCache>> GetByStockNumbersAsync(
        IReadOnlySet<string> stockNumbers,
        CancellationToken cancellationToken = default);
}
