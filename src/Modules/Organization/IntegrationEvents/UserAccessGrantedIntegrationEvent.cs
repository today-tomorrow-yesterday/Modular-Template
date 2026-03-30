using Rtl.Core.Application.EventBus;

namespace Modules.Organization.IntegrationEvents;

// ECST event — initial user cache population with AuthorizedHomeCenters.
// Consumer: Sales (cache.authorized_users)
[EventDetailType("rtl.organization.userAccessGranted")]
public sealed record UserAccessGrantedIntegrationEvent(
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

public sealed record AuthorizedHomeCenterDto(
    int HomeCenterNumber,
    string AssignmentType);
