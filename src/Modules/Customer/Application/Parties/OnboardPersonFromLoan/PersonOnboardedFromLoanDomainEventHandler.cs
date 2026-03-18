using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.OnboardPersonFromLoan;

// Flow: Customer.PartyOnboardedFromLoanDomainEvent → publishes Customer.PartyOnboardedFromLoanIntegrationEvent
internal sealed class PersonOnboardedFromLoanDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<PersonOnboardedFromLoanDomainEventHandler> logger) : DomainEventHandler<PartyOnboardedFromLoanDomainEvent>
{
    public override async Task Handle(
        PartyOnboardedFromLoanDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var party = await partyRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (party is not Person person)
        {
            logger.LogWarning(
                "Party {EntityId} not found or not a Person when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(PartyOnboardedFromLoanDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyOnboardedFromLoanIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                person.PublicId,
                person.HomeCenterNumber,
                person.Name?.FirstName,
                person.Name?.MiddleName,
                person.Name?.LastName,
                person.Name?.NameExtension,
                person.DateOfBirth,
                person.ContactPoints.Select(cp => new ContactPointDto(cp.Type.ToString(), cp.Value, cp.IsPrimary)).ToArray(),
                person.Identifiers.Select(id => new IdentifierDto(id.Type.ToString(), id.Value)).ToArray()),
            cancellationToken);
    }
}
