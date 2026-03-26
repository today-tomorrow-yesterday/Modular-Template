using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Sales.Domain.AuthorizedUsersCache;

// ECST Cache Entity — cache.authorized_users. Populated by Organization events (UserAccessGranted/Changed).
// ALL employees authorized at home centers (22 role types from Organization), NOT just salespeople.
// Filtering by role happens at query time, not cache ingestion.
// Primary use: commission calculation — SalesTeam packages.lines JSONB carries AuthorizedUserId
// referencing cache.authorized_users.Id to resolve EmployeeNumber for iSeries calls.
public sealed class AuthorizedUserCache : ICacheProjection
{
    public int Id { get; set; }

    public Guid RefUserId { get; set; } // Organization User PublicId — upsert key

    [SensitiveData] public string FederatedId { get; set; } = string.Empty;

    public int EmployeeNumber { get; set; } // iSeries: PEMPL

    [SensitiveData] public string FirstName { get; set; } = string.Empty;

    [SensitiveData] public string LastName { get; set; } = string.Empty;

    [SensitiveData] public string DisplayName { get; set; } = string.Empty;

    [SensitiveData] public string? EmailAddress { get; set; }

    public bool IsActive { get; set; } // Only active users can be on sales teams

    public bool IsRetired { get; set; } // Retired users excluded from sales teams

    public int[] AuthorizedHomeCenters { get; set; } = []; // All authorized HomeCenterNumbers (role-expanded)

    public DateTime LastSyncedAtUtc { get; set; }

    public void ApplyChangesFrom(AuthorizedUserCache incoming)
    {
        FederatedId = incoming.FederatedId;
        EmployeeNumber = incoming.EmployeeNumber;
        FirstName = incoming.FirstName;
        LastName = incoming.LastName;
        DisplayName = incoming.DisplayName;
        EmailAddress = incoming.EmailAddress;
        IsActive = incoming.IsActive;
        IsRetired = incoming.IsRetired;
        AuthorizedHomeCenters = incoming.AuthorizedHomeCenters;
        LastSyncedAtUtc = incoming.LastSyncedAtUtc;
    }
}
