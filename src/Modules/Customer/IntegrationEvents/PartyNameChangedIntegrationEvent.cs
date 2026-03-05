using Rtl.Core.Application.EventBus;

namespace Modules.Customer.IntegrationEvents;

// Published when a Party's name changes (Person: first/middle/last, Organization: orgName).
[EventDetailType("rtl.customer.partyNameChanged")]
public sealed record PartyNameChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    Guid PartyId,
    string PartyType,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? NameExtension,
    string? OrganizationName) : IntegrationEvent(Id, OccurredOnUtc);
