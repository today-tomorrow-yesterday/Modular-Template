using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Organization.Domain.Users;

public sealed class User : ICacheProjection
{
    public int Id { get; set; }

    public int RefUserId { get; set; }

    [SensitiveData] public string FirstName { get; set; } = string.Empty;

    [SensitiveData] public string LastName { get; set; } = string.Empty;

    [SensitiveData] public string? MiddleInitial { get; set; }

    [SensitiveData] public string DisplayName { get; set; } = string.Empty;

    [SensitiveData] public string UserName { get; set; } = string.Empty;

    [SensitiveData] public string? EmailAddress { get; set; }

    public int? EmployeeNumber { get; set; }

    [SensitiveData] public string? FederatedId { get; set; }

    [SensitiveData] public string? DistinguishedName { get; set; }

    public string? UserAccountControl { get; set; }

    public string? Title { get; set; }

    public string? Level1 { get; set; }

    public string? Level2 { get; set; }

    public string? Level3 { get; set; }

    public string? Level4 { get; set; }

    public string? PositionNumber { get; set; }

    public int UserRoles { get; set; }

    public bool IsActive { get; set; }

    public bool IsRetired { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation properties
    public ICollection<UserHomeCenter> UserHomeCenters { get; set; } = [];
}
