using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Customers.EventHandlers;

internal sealed class CustomerSalesAssignmentsChangedDomainEventHandler(
    ICustomerRepository customerRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerSalesAssignmentsChangedDomainEventHandler> logger) : DomainEventHandler<CustomerSalesAssignmentsChangedDomainEvent>
{
    public override async Task Handle(
        CustomerSalesAssignmentsChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (customer is null)
        {
            logger.LogError(
                "Customer {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(CustomerSalesAssignmentsChangedDomainEvent));
            return;
        }

        await eventBus.PublishAsync(
            new PartySalesAssignmentsChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                customer.PublicId,
                customer.SalesAssignments
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
