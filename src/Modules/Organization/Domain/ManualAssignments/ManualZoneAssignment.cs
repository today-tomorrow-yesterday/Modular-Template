using Modules.Organization.Domain.Users;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Organization.Domain.ManualAssignments;

public sealed class ManualZoneAssignment : ICacheProjection
{
    public int Id { get; set; }

    public int RefAssignmentId { get; set; }

    public int UserId { get; set; }

    public string Zone { get; set; } = string.Empty;

    public string? StatusCode { get; set; }

    [SensitiveData] public string? Manager { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
