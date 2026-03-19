using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Customers.EventHandlers;

internal sealed class CustomerMailingAddressChangedDomainEventHandler(
    ICustomerRepository customerRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerMailingAddressChangedDomainEventHandler> logger) : DomainEventHandler<CustomerMailingAddressChangedDomainEvent>
{
    public override async Task Handle(
        CustomerMailingAddressChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (customer is null)
        {
            logger.LogError(
                "Customer {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(CustomerMailingAddressChangedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyMailingAddressChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                customer.PublicId,
                customer.MailingAddress is { } a
                    ? new MailingAddressDto(a.AddressLine1, a.AddressLine2, a.City, a.County, a.State, a.Country, a.PostalCode)
                    : null),
            cancellationToken);
    }
}
