using Microsoft.Extensions.Logging;
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
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyCreatedDomainEventHandler> logger) : DomainEventHandler<PartyCreatedDomainEvent>
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
            logger.LogWarning(
                "Party {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(PartyCreatedDomainEvent));
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
                party.ContactPoints.Select(cp => new ContactPointDto(cp.Type.ToString(), cp.Value, cp.IsPrimary)).ToArray(),
                party.Identifiers.Select(id => new IdentifierDto(id.Type.ToString(), id.Value)).ToArray(),
                party.MailingAddress is { } a
                    ? new MailingAddressDto(a.AddressLine1, a.AddressLine2, a.City, a.County, a.State, a.Country, a.PostalCode)
                    : null,
                party.SalesforceUrl),
            cancellationToken);
    }

    private static PersonDataDto? MapPersonData(Party party)
    {
        if (party is not Person person) return null;

        var coBuyer = person.CoBuyer as Person;

        return new PersonDataDto(
            person.Name?.FirstName,
            person.Name?.MiddleName,
            person.Name?.LastName,
            person.Name?.NameExtension,
            person.DateOfBirth,
            [.. person.SalesAssignments
                .Select(sa =>
                {
                    if (sa.SalesPerson is null)
                        throw new InvalidOperationException(
                            $"SalesPerson navigation not loaded for SalesAssignment {sa.Id}. Ensure ThenInclude is used.");

                    return new SalesAssignmentDto(
                        sa.Role.ToString(),
                        sa.SalesPersonId,
                        sa.SalesPerson.Email,
                        sa.SalesPerson.Username,
                        sa.SalesPerson.FirstName,
                        sa.SalesPerson.LastName,
                        sa.SalesPerson.LotNumber,
                        sa.SalesPerson.FederatedId);
                })],
            coBuyer?.PublicId,
            coBuyer?.Name?.FirstName,
            coBuyer?.Name?.MiddleName,
            coBuyer?.Name?.LastName,
            coBuyer?.DateOfBirth);
    }

    private static OrganizationDataDto? MapOrganizationData(Party party)
    {
        if (party is not Organization org) return null;

        return new OrganizationDataDto(org.OrganizationName);
    }
}
