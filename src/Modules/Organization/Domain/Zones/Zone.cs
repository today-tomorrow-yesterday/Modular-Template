using Modules.Organization.Domain.HomeCenters;
using Modules.Organization.Domain.Regions;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Organization.Domain.Zones;

public sealed class Zone : ICacheProjection
{
    public int Id { get; set; }

    public string RefZoneId { get; set; } = string.Empty;

    [SensitiveData] public string? Manager { get; set; }

    public string? StatusCode { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation properties
    public ICollection<HomeCenter> HomeCenters { get; set; } = [];

    public ICollection<Region> Regions { get; set; } = [];
}
