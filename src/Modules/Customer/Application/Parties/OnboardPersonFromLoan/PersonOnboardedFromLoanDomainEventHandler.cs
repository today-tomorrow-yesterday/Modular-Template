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
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<PartyOnboardedFromLoanDomainEvent>
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
            return;
        }

        await eventBus.PublishAsync(
            new PartyOnboardedFromLoanIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                person.Id,
                person.PublicId,
                person.HomeCenterNumber,
                person.Name?.FirstName,
                person.Name?.MiddleName,
                person.Name?.LastName,
                person.Name?.NameExtension,
                person.DateOfBirth,
                person.ContactPoints.ToIntegrationDtos(),
                person.Identifiers.ToIntegrationDtos()),
            cancellationToken);
    }
}
