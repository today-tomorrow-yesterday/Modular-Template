using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Customers.EventHandlers;

internal sealed class CustomerLifecycleAdvancedDomainEventHandler(
    ICustomerRepository customerRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerLifecycleAdvancedDomainEventHandler> logger) : DomainEventHandler<CustomerLifecycleAdvancedDomainEvent>
{
    public override async Task Handle(
        CustomerLifecycleAdvancedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (customer is null)
        {
            logger.LogError(
                "Customer {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(CustomerLifecycleAdvancedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyLifecycleAdvancedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                customer.PublicId,
                domainEvent.NewStage.ToString()),
            cancellationToken);
    }
}
