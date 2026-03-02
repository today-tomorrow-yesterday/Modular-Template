using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Sales.Application.PartiesCache.UpsertPartyCache;
using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Customer;

// Flow: Customer.PartyOnboardedFromLoanIntegrationEvent → Sales.UpsertPartyCacheCommand
internal sealed class PartyOnboardedFromLoanIntegrationEventHandler(
    ISender sender,
    ILogger<PartyOnboardedFromLoanIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyOnboardedFromLoanIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyOnboardedFromLoanIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing PartyOnboardedFromLoan: PartyId={PartyId}",
            integrationEvent.PartyId);

        var displayName = $"{integrationEvent.FirstName} {integrationEvent.LastName}".Trim();

        var email = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Email")?.Value;
        var phone = integrationEvent.ContactPoints
            .FirstOrDefault(cp => cp.Type == "Phone")?.Value;

        var partyCache = new PartyCache
        {
            RefPartyId = integrationEvent.PartyId,
            RefPublicId = integrationEvent.PublicId,
            PartyType = PartyType.Person,
            LifecycleStage = LifecycleStage.Customer,
            HomeCenterNumber = integrationEvent.HomeCenterNumber,
            DisplayName = displayName
        };

        var personCache = new PartyPersonCache
        {
            FirstName = integrationEvent.FirstName ?? string.Empty,
            MiddleName = integrationEvent.MiddleName,
            LastName = integrationEvent.LastName ?? string.Empty,
            Email = email,
            Phone = phone
        };

        await sender.Send(
            new UpsertPartyCacheCommand(partyCache, personCache, OrganizationCache: null),
            cancellationToken);
    }
}
