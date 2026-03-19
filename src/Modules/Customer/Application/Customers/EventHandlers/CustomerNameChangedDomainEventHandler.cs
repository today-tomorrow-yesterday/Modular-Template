using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Customers.EventHandlers;

internal sealed class CustomerNameChangedDomainEventHandler(
    ICustomerRepository customerRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerNameChangedDomainEventHandler> logger) : DomainEventHandler<CustomerNameChangedDomainEvent>
{
    public override async Task Handle(
        CustomerNameChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (customer is null)
        {
            logger.LogError(
                "Customer {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(CustomerNameChangedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyNameChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                customer.PublicId,
                "Person",
                customer.Name?.FirstName,
                customer.Name?.MiddleName,
                customer.Name?.LastName,
                customer.Name?.NameExtension,
                null),
            cancellationToken);
    }
}
