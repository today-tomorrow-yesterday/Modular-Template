using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.EventHandlers;

// Flow: Customer.PartyMailingAddressChangedDomainEvent → publishes Customer.PartyMailingAddressChangedIntegrationEvent
internal sealed class PartyMailingAddressChangedDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<PartyMailingAddressChangedDomainEvent>
{
    public override async Task Handle(
        PartyMailingAddressChangedDomainEvent domainEvent,
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
            new PartyMailingAddressChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                party.Id,
                party.PublicId,
                party.MailingAddress.ToIntegrationDto()),
            cancellationToken);
    }
}
