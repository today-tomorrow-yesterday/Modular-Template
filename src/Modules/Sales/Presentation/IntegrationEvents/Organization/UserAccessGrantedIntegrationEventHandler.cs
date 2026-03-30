using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Organization.IntegrationEvents;
using Modules.Sales.Application.AuthorizedUsersCache.UpsertAuthorizedUserCache;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Organization;

// Flow: Organization.UserAccessGranted (EventBridge) → Send UpsertAuthorizedUserCacheCommand → Upsert cache.authorized_users
// Initial user cache population with AuthorizedHomeCenters (all 22 role types, not just salespeople).
internal sealed class UserAccessGrantedIntegrationEventHandler(
    ISender sender,
    ILogger<UserAccessGrantedIntegrationEventHandler> logger)
    : IntegrationEventHandler<UserAccessGrantedIntegrationEvent>
{
    public override async Task HandleAsync(
        UserAccessGrantedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing UserAccessGranted: PublicUserId={PublicUserId}, Employee={EmployeeNumber}, HCs={HomeCenterCount}",
            integrationEvent.PublicUserId,
            integrationEvent.EmployeeNumber,
            integrationEvent.AuthorizedHomeCenters.Count);

        var cache = MapToCache(integrationEvent);

        await sender.Send(
            new UpsertAuthorizedUserCacheCommand(cache),
            cancellationToken);
    }

    private static AuthorizedUserCache MapToCache(UserAccessGrantedIntegrationEvent e) => new()
    {
        RefUserId = e.PublicUserId,
        FederatedId = e.FederatedId,
        EmployeeNumber = e.EmployeeNumber,
        FirstName = e.FirstName,
        LastName = e.LastName,
        DisplayName = e.DisplayName,
        EmailAddress = e.EmailAddress,
        IsActive = e.IsActive,
        IsRetired = e.IsRetired,
        AuthorizedHomeCenters = e.AuthorizedHomeCenters.Select(hc => hc.HomeCenterNumber).ToArray()
    };
}
