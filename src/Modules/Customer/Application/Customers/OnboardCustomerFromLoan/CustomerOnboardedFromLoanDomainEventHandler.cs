using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Customers.OnboardCustomerFromLoan;

internal sealed class CustomerOnboardedFromLoanDomainEventHandler(
    ICustomerRepository customerRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerOnboardedFromLoanDomainEventHandler> logger) : DomainEventHandler<CustomerOnboardedFromLoanDomainEvent>
{
    public override async Task Handle(
        CustomerOnboardedFromLoanDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (customer is null)
        {
            logger.LogError(
                "Customer {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(CustomerOnboardedFromLoanDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartyOnboardedFromLoanIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                customer.PublicId,
                customer.HomeCenterNumber,
                customer.Name?.FirstName,
                customer.Name?.MiddleName,
                customer.Name?.LastName,
                customer.Name?.NameExtension,
                customer.DateOfBirth,
                customer.ContactPoints.Select(cp => new ContactPointDto(cp.Type.ToString(), cp.Value, cp.IsPrimary)).ToArray(),
                customer.Identifiers.Select(id => new IdentifierDto(id.Type.ToString(), id.Value)).ToArray()),
            cancellationToken);
    }
}
