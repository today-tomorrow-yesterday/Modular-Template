using Modules.Organization.Domain.HomeCenters;
using Modules.Organization.Domain.Users;
using Rtl.Core.Domain.Caching;

namespace Modules.Organization.Domain.ManualAssignments;

public sealed class ManualHomeCenterAssignment : ICacheProjection
{
    public int Id { get; set; }

    public int RefAssignmentId { get; set; }

    public int UserId { get; set; }

    public int HomeCenterId { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;

    public HomeCenter HomeCenter { get; set; } = null!;
}
