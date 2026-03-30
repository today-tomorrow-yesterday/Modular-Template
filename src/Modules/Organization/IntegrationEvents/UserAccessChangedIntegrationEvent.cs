using Rtl.Core.Application.EventBus;

namespace Modules.Organization.IntegrationEvents;

// ECST event — updated user authorization and home center assignments (all 22 role types).
// Consumer: Sales (cache.authorized_users)
[EventDetailType("rtl.organization.userAccessChanged")]
public sealed record UserAccessChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PublicUserId,
    string FederatedId,
    string FirstName,
    string LastName,
    string DisplayName,
    string? EmailAddress,
    int EmployeeNumber,
    IReadOnlyCollection<AuthorizedHomeCenterDto> AuthorizedHomeCenters,
    bool IsActive,
    bool IsRetired) : IntegrationEvent(Id, OccurredOnUtc);
