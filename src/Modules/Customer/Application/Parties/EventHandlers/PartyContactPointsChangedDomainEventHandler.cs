using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.EventHandlers;

// Flow: Customer.PartyContactPointsChangedDomainEvent → publishes Customer.PartyContactPointsChangedIntegrationEvent
internal sealed class PartyContactPointsChangedDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyContactPointsChangedDomainEventHandler> logger) : DomainEventHandler<PartyContactPointsChangedDomainEvent>
{
    public override async Task Handle(
        PartyContactPointsChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var party = await partyRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (party is null)
        {
            logger.LogWarning(
                "Party {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(PartyContactPointsChangedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyContactPointsChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                party.PublicId,
                party.ContactPoints.Select(cp => new ContactPointDto(cp.Type.ToString(), cp.Value, cp.IsPrimary)).ToArray()),
            cancellationToken);
    }
}
