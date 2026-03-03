using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.SyncPartyFromCrm;

// Flow: Customer.PartyCreatedDomainEvent → publishes Customer.PartyCreatedIntegrationEvent
internal sealed class PartyCreatedDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<PartyCreatedDomainEvent>
{
    public override async Task Handle(
        PartyCreatedDomainEvent domainEvent,
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
            new PartyCreatedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                party.PublicId,
                party.PartyType.ToString(),
                party.LifecycleStage.ToString(),
                party.HomeCenterNumber,
                MapPersonData(party),
                MapOrganizationData(party),
                party.ContactPoints.ToIntegrationDtos(),
                party.Identifiers.ToIntegrationDtos(),
                party.MailingAddress.ToIntegrationDto(),
                party.SalesforceUrl),
            cancellationToken);
    }

    private static PersonDataDto? MapPersonData(Party party)
    {
        if (party is not Person person) return null;

        return new PersonDataDto(
            person.Name?.FirstName,
            person.Name?.MiddleName,
            person.Name?.LastName,
            person.Name?.NameExtension,
            person.DateOfBirth,
            person.SalesAssignments
                .Select(sa => new SalesAssignmentDto(
                    sa.Role.ToString(),
                    sa.SalesPersonId,
                    sa.SalesPerson.Email,
                    sa.SalesPerson.Username,
                    sa.SalesPerson.FirstName,
                    sa.SalesPerson.LastName,
                    sa.SalesPerson.LotNumber,
                    sa.SalesPerson.FederatedId))
                .ToArray(),
            (person.CoBuyer as Person)?.PublicId,
            (person.CoBuyer as Person)?.Name?.FirstName,
            (person.CoBuyer as Person)?.Name?.LastName);
    }

    private static OrganizationDataDto? MapOrganizationData(Party party)
    {
        if (party is not Organization org) return null;

        return new OrganizationDataDto(org.OrganizationName);
    }
}
