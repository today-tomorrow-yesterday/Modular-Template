using Rtl.Core.Application.EventBus;

namespace Modules.Sales.IntegrationEvents;

// Published by Sales when a delivery address is first created for a sale.
// Intended consumer: Customers module (sync address to CRM) — not yet implemented.
// Same payload as DeliveryAddressChangedIntegrationEvent, different event name.
public sealed record DeliveryAddressCreatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int SaleId,
    Guid SalePublicId,
    int? PartyId,
    string? OccupancyType,
    bool IsWithinCityLimits,
    string? AddressLine1,
    string? City,
    string? County,
    string? State,
    string? PostalCode) : IntegrationEvent(Id, OccurredOnUtc);
