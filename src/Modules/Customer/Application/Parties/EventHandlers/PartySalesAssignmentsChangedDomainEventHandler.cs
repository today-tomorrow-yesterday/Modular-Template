using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Parties.EventHandlers;

// Flow: Customer.PartySalesAssignmentsChangedDomainEvent → publishes Customer.PartySalesAssignmentsChangedIntegrationEvent
internal sealed class PartySalesAssignmentsChangedDomainEventHandler(
    IPartyRepository partyRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartySalesAssignmentsChangedDomainEventHandler> logger) : DomainEventHandler<PartySalesAssignmentsChangedDomainEvent>
{
    public override async Task Handle(
        PartySalesAssignmentsChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var party = await partyRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (party is not Person person)
        {
            logger.LogWarning(
                "Party {EntityId} not found or not a Person when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(PartySalesAssignmentsChangedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartySalesAssignmentsChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                person.PublicId,
                person.SalesAssignments
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
                    })
                    .ToArray()),
            cancellationToken);
    }
}
