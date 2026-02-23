using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.EventHandlers;

// Flow: Customer.PartyHomeCenterChangedDomainEvent → publishes Customer.PartyHomeCenterChangedIntegrationEvent
internal sealed class PartyHomeCenterChangedDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<PartyHomeCenterChangedDomainEvent>
{
    public override async Task Handle(
        PartyHomeCenterChangedDomainEvent domainEvent,
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
            new PartyHomeCenterChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                party.Id,
                party.PublicId,
                domainEvent.NewHomeCenterNumber),
            cancellationToken);
    }
}
