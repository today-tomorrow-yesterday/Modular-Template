using Modules.Customer.Application.Parties.GetPartyByPublicId;
using Modules.Customer.Domain.Parties.Entities;

namespace Modules.Customer.Application.Parties;

internal static class PartyResponseMapper
{
    public static PartyResponse MapToResponse(Party party)
    {
        return new PartyResponse(
            party.PublicId,
            party.PartyType.ToString(),
            party.LifecycleStage.ToString(),
            party.HomeCenterNumber,
            MapPersonData(party),
            MapOrganizationData(party),
            party.ContactPoints.ToResponses(),
            party.Identifiers.ToResponses(),
            party.MailingAddress.ToResponse(),
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
            person.CoBuyerPartyId,
            person.CoBuyer?.PublicId,
            coBuyer?.Name?.FirstName,
            coBuyer?.Name?.MiddleName,
            coBuyer?.Name?.LastName,
            coBuyer?.DateOfBirth);
    }

    private static SalesAssignmentResponse[] MapSalesAssignments(Person person)
    {
        return person.SalesAssignments
            .Select(sa => new SalesAssignmentResponse(
                sa.Role.ToString(),
                new SalesPersonResponse(
                    sa.SalesPerson.Id,
                    sa.SalesPerson.Email,
                    sa.SalesPerson.Username,
                    sa.SalesPerson.FirstName,
                    sa.SalesPerson.LastName,
                    sa.SalesPerson.LotNumber,
                    sa.SalesPerson.FederatedId)))
            .ToArray();
    }

    private static OrganizationDataResponse? MapOrganizationData(Party party)
    {
        if (party is not Organization org) return null;

        return new OrganizationDataResponse(org.OrganizationName);
    }
}
