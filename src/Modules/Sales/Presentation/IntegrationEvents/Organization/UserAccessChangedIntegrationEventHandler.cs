using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Organization.IntegrationEvents;
using Modules.Sales.Application.AuthorizedUsersCache.UpsertAuthorizedUserCache;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Organization;

// Flow: Organization.UserAccessChanged (EventBridge) → Send UpsertAuthorizedUserCacheCommand → Upsert cache.authorized_users
// Updated user authorization and home center assignments (all 22 role types).
internal sealed class UserAccessChangedIntegrationEventHandler(
    ISender sender,
    ILogger<UserAccessChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<UserAccessChangedIntegrationEvent>
{
    public async Task HandleAsync(
        UserAccessChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing UserAccessChanged: UserId={UserId}, Employee={EmployeeNumber}, HCs={HomeCenterCount}, Active={IsActive}, Retired={IsRetired}",
            integrationEvent.UserId,
            integrationEvent.EmployeeNumber,
            integrationEvent.AuthorizedHomeCenters.Count,
            integrationEvent.IsActive,
            integrationEvent.IsRetired);

        var cache = MapToCache(integrationEvent);

        await sender.Send(
            new UpsertAuthorizedUserCacheCommand(cache),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((UserAccessChangedIntegrationEvent)integrationEvent, cancellationToken);

    private static AuthorizedUserCache MapToCache(UserAccessChangedIntegrationEvent e) => new()
    {
        RefUserId = e.UserId,
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
