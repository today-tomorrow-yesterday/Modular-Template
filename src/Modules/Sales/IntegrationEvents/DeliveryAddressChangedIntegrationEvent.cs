using Rtl.Core.Application.EventBus;

namespace Modules.Sales.IntegrationEvents;

// Published by Sales when a delivery address is updated.
// Intended consumer: Customers module (sync address to CRM) — not yet implemented.
public sealed record DeliveryAddressChangedIntegrationEvent(
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
