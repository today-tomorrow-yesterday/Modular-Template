using Modules.Organization.Domain.HomeCenters;
using Rtl.Core.Domain.Caching;

namespace Modules.Organization.Domain.Users;

public sealed class UserHomeCenter : ICacheProjection
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int HomeCenterId { get; set; }

    public string AssignmentType { get; set; } = string.Empty;

    public DateTimeOffset AssignedAt { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    public HomeCenter HomeCenter { get; set; } = null!;
}
