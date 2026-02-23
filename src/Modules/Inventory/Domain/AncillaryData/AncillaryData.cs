using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Inventory.Domain.AncillaryData;

public sealed class AncillaryData : ICacheProjection
{
    public int Id { get; set; }

    public int RefHomeCenterNumber { get; set; }

    public string RefStockNumber { get; set; } = string.Empty;

    [SensitiveData] public string? CustomerName { get; set; }

    public DateTime? PackageReceivedDate { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }
}
