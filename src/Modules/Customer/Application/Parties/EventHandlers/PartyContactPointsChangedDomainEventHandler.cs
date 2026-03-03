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
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<PartyContactPointsChangedDomainEvent>
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
            return;
        }

        await eventBus.PublishAsync(
            new PartyContactPointsChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                party.PublicId,
                party.ContactPoints.ToIntegrationDtos()),
            cancellationToken);
    }
}
