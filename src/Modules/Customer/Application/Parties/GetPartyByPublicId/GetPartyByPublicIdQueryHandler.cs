using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Errors;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Application.Parties.GetPartyByPublicId;

// Flow: GET /api/v1/parties/{publicId} → query Customer.parties →
//   map Party aggregate to PartyResponse (polymorphic Person/Organization)
internal sealed class GetPartyByPublicIdQueryHandler(
    IPartyRepository partyRepository)
    : IQueryHandler<GetPartyByPublicIdQuery, PartyResponse>
{
    public async Task<Result<PartyResponse>> Handle(
        GetPartyByPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        var party = await partyRepository.GetByPublicIdAsync(
            request.PublicId,
            cancellationToken);

        if (party is null)
        {
            return Result.Failure<PartyResponse>(
                PartyErrors.NotFoundByPublicId(request.PublicId));
        }

        return MapToResponse(party);
    }

    private static PartyResponse MapToResponse(Party party)
    {
        return new PartyResponse(
            party.PublicId,
            party.PartyType.ToString(),
            party.LifecycleStage.ToString(),
            party.HomeCenterNumber,
            MapPersonData(party),
            MapOrganizationData(party),
            MapContactPoints(party.ContactPoints),
            MapIdentifiers(party.Identifiers),
            MapMailingAddress(party.MailingAddress),
            party.SalesforceUrl,
            party.LastSyncedAtUtc);
    }

    private static PersonDataResponse? MapPersonData(Party party)
    {
        if (party is not Person person) return null;

        var coBuyer = person.CoBuyer as Person;

        return new PersonDataResponse(
            person.Name?.FirstName,
            person.Name?.MiddleName,
            person.Name?.LastName,
            person.Name?.NameExtension,
            person.DateOfBirth,
            MapSalesAssignments(person),
            person.CoBuyer?.PublicId,
            coBuyer?.Name?.FirstName,
            coBuyer?.Name?.MiddleName,
            coBuyer?.Name?.LastName,
            coBuyer?.DateOfBirth);
    }

    private static SalesAssignmentResponse[] MapSalesAssignments(Person person)
    {
        return [.. person.SalesAssignments
            .Select(sa =>
            {
                if (sa.SalesPerson is null)
                    throw new InvalidOperationException(
                        $"SalesPerson navigation not loaded for SalesAssignment {sa.Id}. Ensure ThenInclude is used.");

                return new SalesAssignmentResponse(
                    sa.Role.ToString(),
                    new SalesPersonResponse(
                        sa.SalesPerson.Id,
                        sa.SalesPerson.Email,
                        sa.SalesPerson.Username,
                        sa.SalesPerson.FirstName,
                        sa.SalesPerson.LastName,
                        sa.SalesPerson.LotNumber,
                        sa.SalesPerson.FederatedId));
            })];
    }

    private static OrganizationDataResponse? MapOrganizationData(Party party)
    {
        if (party is not Organization org) return null;

        return new OrganizationDataResponse(org.OrganizationName);
    }

    private static ContactPointResponse[] MapContactPoints(IReadOnlyCollection<ContactPoint> contactPoints)
    {
        return [.. contactPoints
            .Select(cp => new ContactPointResponse(cp.Type.ToString(), cp.Value, cp.IsPrimary))];
    }

    private static IdentifierResponse[] MapIdentifiers(IReadOnlyCollection<PartyIdentifier> identifiers)
    {
        return [.. identifiers
            .Select(id => new IdentifierResponse(id.Type.ToString(), id.Value))];
    }

    private static MailingAddressResponse? MapMailingAddress(MailingAddress? address)
    {
        if (address is null) return null;

        return new MailingAddressResponse(
            address.AddressLine1, address.AddressLine2,
            address.City, address.County, address.State,
            address.Country, address.PostalCode);
    }
}
