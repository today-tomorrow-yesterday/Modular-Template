using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.EventHandlers;

// Flow: Customer.PartyNameChangedDomainEvent → publishes Customer.PartyNameChangedIntegrationEvent
internal sealed class PartyNameChangedDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyNameChangedDomainEventHandler> logger) : DomainEventHandler<PartyNameChangedDomainEvent>
{
    public override async Task Handle(
        PartyNameChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var party = await partyRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (party is null)
        {
            logger.LogWarning(
                "Party {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(PartyNameChangedDomainEvent));
            return;
        }

        var (firstName, middleName, lastName, nameExtension, orgName) = party switch
        {
            Person p => (p.Name?.FirstName, p.Name?.MiddleName, p.Name?.LastName, p.Name?.NameExtension, (string?)null),
            Organization o => (null, null, null, null, o.OrganizationName),
            _ => (null, null, null, null, (string?)null)
        };

        await eventBus.PublishAsync(
            new PartyNameChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                party.PublicId,
                party.PartyType.ToString(),
                firstName,
                middleName,
                lastName,
                nameExtension,
                orgName),
            cancellationToken);
    }
}
