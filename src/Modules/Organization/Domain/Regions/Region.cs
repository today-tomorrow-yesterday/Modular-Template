using Modules.Organization.Domain.HomeCenters;
using Modules.Organization.Domain.Zones;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Organization.Domain.Regions;

public sealed class Region : ICacheProjection
{
    public int Id { get; set; }

    public string RefRegionId { get; set; } = string.Empty;

    public string? Description { get; set; }

    [SensitiveData] public string? Manager { get; set; }

    public int? DummyHomeCenterNumber { get; set; }

    public string? StatusCode { get; set; }

    public int? ZoneId { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation properties
    public Zone? Zone { get; set; }

    public ICollection<HomeCenter> HomeCenters { get; set; } = [];
}
