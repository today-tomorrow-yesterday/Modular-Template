using Modules.Organization.Domain.Regions;
using Modules.Organization.Domain.Users;
using Modules.Organization.Domain.Zones;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Organization.Domain.HomeCenters;

public sealed class HomeCenter : ICacheProjection
{
    public int Id { get; set; }

    public int RefHomeCenterNumber { get; set; }

    public string LotMdlr { get; set; } = string.Empty;

    public string LotName { get; set; } = string.Empty;

    public string? LotDba { get; set; }

    public string? Brand { get; set; }

    public string LotStatus { get; set; } = string.Empty;

    [SensitiveData] public string? Address1 { get; set; }

    [SensitiveData] public string? Address2 { get; set; }

    [SensitiveData] public string? City { get; set; }

    [SensitiveData] public string? StateCode { get; set; }

    [SensitiveData] public string? Zip { get; set; }

    [SensitiveData] public string? MailingAddress1 { get; set; }

    [SensitiveData] public string? MailingAddress2 { get; set; }

    [SensitiveData] public string? MailingCity { get; set; }

    [SensitiveData] public string? MailingStateCode { get; set; }

    [SensitiveData] public string? MailingZip { get; set; }

    public int? ZoneId { get; set; }

    public int? RegionId { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    [SensitiveData] public string? AreaCode { get; set; }

    [SensitiveData] public string? PhoneNumber { get; set; }

    public int? ManagerEmployeeNumber { get; set; }

    public bool IsActive { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation properties
    public Zone? Zone { get; set; }

    public Region? Region { get; set; }

    public ICollection<UserHomeCenter> UserHomeCenters { get; set; } = [];
}
