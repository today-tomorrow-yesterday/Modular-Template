using Modules.Customer.Application.Parties.GetPartyByPublicId;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.IntegrationEvents;

namespace Modules.Customer.Application.Parties;

internal static class PartyMappingExtensions
{
    // ── Integration Event DTO mappings (used by domain event handlers) ──

    internal static ContactPointDto[] ToIntegrationDtos(this IReadOnlyCollection<ContactPoint> contactPoints)
    {
        return contactPoints
            .Select(cp => new ContactPointDto(cp.Type.ToString(), cp.Value, cp.IsPrimary))
            .ToArray();
    }

    internal static IdentifierDto[] ToIntegrationDtos(this IReadOnlyCollection<PartyIdentifier> identifiers)
    {
        return identifiers
            .Select(id => new IdentifierDto(id.Type.ToString(), id.Value))
            .ToArray();
    }

    internal static MailingAddressDto? ToIntegrationDto(this MailingAddress? address)
    {
        return address is null
            ? null
            : new MailingAddressDto(
                address.AddressLine1, address.AddressLine2,
                address.City, address.County, address.State,
                address.Country, address.PostalCode);
    }

    // ── API Response DTO mappings (used by query response mapper) ──

    internal static ContactPointResponse[] ToResponses(this IReadOnlyCollection<ContactPoint> contactPoints)
    {
        return contactPoints
            .Select(cp => new ContactPointResponse(cp.Type.ToString(), cp.Value, cp.IsPrimary))
            .ToArray();
    }

    internal static IdentifierResponse[] ToResponses(this IReadOnlyCollection<PartyIdentifier> identifiers)
    {
        return identifiers
            .Select(id => new IdentifierResponse(id.Type.ToString(), id.Value))
            .ToArray();
    }

    internal static MailingAddressResponse? ToResponse(this MailingAddress? address)
    {
        return address is null
            ? null
            : new MailingAddressResponse(
                address.AddressLine1, address.AddressLine2,
                address.City, address.County, address.State,
                address.Country, address.PostalCode);
    }
}
