using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Inventory.Domain.SaleSummariesCache;

public sealed class SaleSummaryCache : ICacheProjection
{
    public int Id { get; set; }

    public string RefStockNumber { get; set; } = string.Empty;

    public int? SaleId { get; set; }

    [SensitiveData] public string? CustomerName { get; set; }

    public DateTime? ReceivedInDate { get; set; }

    [SensitiveData] public decimal? OriginalRetailPrice { get; set; }

    [SensitiveData] public decimal? CurrentRetailPrice { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }
}
