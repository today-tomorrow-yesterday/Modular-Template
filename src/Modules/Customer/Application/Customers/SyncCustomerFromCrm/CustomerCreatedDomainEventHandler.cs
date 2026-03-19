using Microsoft.Extensions.Logging;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Events;
using Modules.Customer.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Customer.Application.Customers.SyncCustomerFromCrm;

internal sealed class CustomerCreatedDomainEventHandler(
    ICustomerRepository customerRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider,
    ILogger<CustomerCreatedDomainEventHandler> logger) : DomainEventHandler<CustomerCreatedDomainEvent>
{
    public override async Task Handle(
        CustomerCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdWithDetailsAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (customer is null)
        {
            logger.LogError(
                "Customer {EntityId} not found when handling {Event}. Integration event discarded.",
                domainEvent.EntityId, nameof(CustomerCreatedDomainEvent));
            return;
        }

        var coBuyer = customer.CoBuyer;

        await eventBus.PublishAsync(
            new CustomerCreatedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                customer.PublicId,
                customer.LifecycleStage.ToString(),
                customer.HomeCenterNumber,
                customer.Name?.FirstName,
                customer.Name?.MiddleName,
                customer.Name?.LastName,
                customer.Name?.NameExtension,
                customer.DateOfBirth,
                [.. customer.SalesAssignments.Select(sa =>
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
                coBuyer?.DateOfBirth,
                customer.ContactPoints.Select(cp => new ContactPointDto(cp.Type.ToString(), cp.Value, cp.IsPrimary)).ToArray(),
                customer.Identifiers.Select(id => new IdentifierDto(id.Type.ToString(), id.Value)).ToArray(),
                customer.MailingAddress is { } a
                    ? new MailingAddressDto(a.AddressLine1, a.AddressLine2, a.City, a.County, a.State, a.Country, a.PostalCode)
                    : null,
                customer.SalesforceUrl),
            cancellationToken);
    }
}
