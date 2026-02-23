using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Inventory.Domain.LandCosts;

public sealed class LandCost : ICacheProjection
{
    public int Id { get; set; }

    public int RefHomeCenterNumber { get; set; }

    public string RefStockNumber { get; set; } = string.Empty;

    [SensitiveData] public decimal? AddToTotal { get; set; }

    [SensitiveData] public decimal? FurnitureTotal { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }
}
