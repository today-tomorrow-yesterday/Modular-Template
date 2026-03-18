using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.EventHandlers;

// Flow: Customer.PartyLifecycleAdvancedDomainEvent → publishes Customer.PartyLifecycleAdvancedIntegrationEvent
internal sealed class PartyLifecycleAdvancedDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyLifecycleAdvancedDomainEventHandler> logger) : DomainEventHandler<PartyLifecycleAdvancedDomainEvent>
{
    public override async Task Handle(
        PartyLifecycleAdvancedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var party = await partyRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (party is null)
        {
            logger.LogWarning(
                "Party {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(PartyLifecycleAdvancedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyLifecycleAdvancedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                party.PublicId,
                domainEvent.NewStage.ToString()),
            cancellationToken);
    }
}
