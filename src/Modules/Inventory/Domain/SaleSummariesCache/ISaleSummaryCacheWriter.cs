namespace Modules.Inventory.Domain.SaleSummariesCache;

public interface ISaleSummaryCacheWriter
{
    Task UpsertAsync(SaleSummaryCache cache, CancellationToken cancellationToken = default);
}
