using Rtl.Core.Domain.Caching;

namespace Modules.Inventory.Domain.HomeCentersCache;

public sealed class HomeCenterCache : ICacheProjection
{
    public int Id { get; set; }

    public int RefHomeCenterNumber { get; set; }

    public string LotName { get; set; } = string.Empty;

    public string? StateCode { get; set; }

    public int? ZoneId { get; set; }

    public int? RegionId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }
}
