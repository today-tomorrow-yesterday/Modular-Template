using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Customers.EventHandlers;

internal sealed class CustomerCoBuyerChangedDomainEventHandler(
    ICustomerRepository customerRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerCoBuyerChangedDomainEventHandler> logger) : DomainEventHandler<CustomerCoBuyerChangedDomainEvent>
{
    public override async Task Handle(
        CustomerCoBuyerChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (customer is null)
        {
            logger.LogError(
                "Customer {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(CustomerCoBuyerChangedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyCoBuyerChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                customer.PublicId,
                customer.CoBuyer?.PublicId,
                customer.CoBuyer?.Name?.FirstName,
                customer.CoBuyer?.Name?.LastName),
            cancellationToken);
    }
}
