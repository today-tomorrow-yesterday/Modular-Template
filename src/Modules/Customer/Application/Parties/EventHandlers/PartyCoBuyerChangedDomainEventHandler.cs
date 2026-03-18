using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.EventHandlers;

// Flow: Customer.PartyCoBuyerChangedDomainEvent → publishes Customer.PartyCoBuyerChangedIntegrationEvent
internal sealed class PartyCoBuyerChangedDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyCoBuyerChangedDomainEventHandler> logger) : DomainEventHandler<PartyCoBuyerChangedDomainEvent>
{
    public override async Task Handle(
        PartyCoBuyerChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var party = await partyRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (party is not Person person)
        {
            logger.LogWarning(
                "Party {EntityId} not found or not a Person when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(PartyCoBuyerChangedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyCoBuyerChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                person.PublicId,
                (person.CoBuyer as Person)?.PublicId,
                (person.CoBuyer as Person)?.Name?.FirstName,
                (person.CoBuyer as Person)?.Name?.LastName),
            cancellationToken);
    }
}
