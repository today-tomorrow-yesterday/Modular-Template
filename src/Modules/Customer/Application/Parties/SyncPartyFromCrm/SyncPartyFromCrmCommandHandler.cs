using Modules.Customer.Domain;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Errors;
using Modules.Customer.Domain.SalesPersons;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Application.Parties.SyncPartyFromCrm;

// Flow: Customer.SyncPartyFromCrmCommand → upsert Customer.parties → raises Customer.PartyCreatedDomainEvent
internal sealed class SyncPartyFromCrmCommandHandler(
    IPartyRepository partyRepository,
    ISalesPersonRepository salesPersonRepository,
    IUnitOfWork<ICustomerModule> unitOfWork)
    : ICommandHandler<SyncPartyFromCrmCommand>
{
    public async Task<Result> Handle(
        SyncPartyFromCrmCommand request,
        CancellationToken cancellationToken)
    {
        // Upsert SalesPersons (Person path only)
        if (request.PartyType == PartyType.Person && request.PersonData is not null)
        {
            await UpsertSalesPersonsAsync(request.PersonData, cancellationToken);
        }

        var existing = await partyRepository.GetByIdAsync(request.PartyId, cancellationToken);

        if (existing is not null)
        {
            UpdateExistingParty(existing, request);
        }
        else
        {
            var createResult = CreateNewParty(request);
            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }

            partyRepository.Add(createResult.Value);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static void UpdateExistingParty(Party existing, SyncPartyFromCrmCommand request)
    {
        var mailingAddress = MapMailingAddress(request.MailingAddress);

        // TPH: the existing entity is already the correct derived type
        switch (existing)
        {
            case Person person when request.PersonData is not null:
                var assignments = MapSalesAssignments(request.PersonData);
                person.UpdateFromCrmSync(
                    request.PersonData.FirstName is not null
                        ? PersonName.Create(
                            request.PersonData.FirstName,
                            request.PersonData.MiddleName,
                            request.PersonData.LastName,
                            request.PersonData.NameExtension)
                        : null,
                    request.PersonData.DateOfBirth,
                    assignments,
                    request.HomeCenterNumber,
                    request.SalesforceUrl,
                    mailingAddress,
                    request.LastModifiedOn);
                break;

            case Organization org when request.OrganizationData is not null:
                org.UpdateFromCrmSync(
                    request.OrganizationData.OrganizationName,
                    request.HomeCenterNumber,
                    request.SalesforceUrl,
                    mailingAddress,
                    request.LastModifiedOn);
                break;
        }

        // Sync contact points — replace all (CDC is full-state, not delta)
        SyncContactPoints(existing, request.ContactPoints);

        // Sync identifiers — upsert by type
        foreach (var identifier in request.Identifiers)
        {
            existing.AddIdentifier(identifier.Type, identifier.Value);
        }
    }

    private static Result<Party> CreateNewParty(SyncPartyFromCrmCommand request)
    {
        var mailingAddress = MapMailingAddress(request.MailingAddress);

        var personAssignments = request.PersonData is not null
            ? MapSalesAssignments(request.PersonData)
            : [];

        Party? party = request.PartyType switch
        {
            PartyType.Person when request.PersonData is not null => Person.SyncFromCrm(
                request.PartyId,
                request.HomeCenterNumber,
                request.LifecycleStage,
                request.PersonData.FirstName is not null
                    ? PersonName.Create(
                        request.PersonData.FirstName,
                        request.PersonData.MiddleName,
                        request.PersonData.LastName,
                        request.PersonData.NameExtension)
                    : null,
                request.PersonData.DateOfBirth,
                personAssignments,
                request.SalesforceUrl,
                mailingAddress,
                request.CreatedOn,
                request.LastModifiedOn),

            PartyType.Organization when request.OrganizationData is not null => Organization.SyncFromCrm(
                request.PartyId,
                request.HomeCenterNumber,
                request.LifecycleStage,
                request.OrganizationData.OrganizationName,
                request.SalesforceUrl,
                mailingAddress,
                request.CreatedOn,
                request.LastModifiedOn),

            _ => null
        };

        if (party is null)
        {
            return Result.Failure<Party>(PartyErrors.InvalidPartyTypeData(request.PartyType));
        }

        // Add contact points after creation
        foreach (var cp in request.ContactPoints)
        {
            party.AddContactPoint(cp.Type, cp.Value, cp.IsPrimary);
        }

        // Add identifiers after creation
        foreach (var id in request.Identifiers)
        {
            party.AddIdentifier(id.Type, id.Value);
        }

        return party;
    }

    private static void SyncContactPoints(Party party, SyncContactPointDto[] incoming)
    {
        // CDC is full-state — replace all contact points with incoming set.
        // Stale contacts not in the incoming payload are removed.
        party.ReplaceContactPoints(
            incoming.Select(cp => (cp.Type, cp.Value, cp.IsPrimary)));
    }

    private static MailingAddress? MapMailingAddress(SyncMailingAddressDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return MailingAddress.Create(
            dto.AddressLine1,
            dto.AddressLine2,
            dto.City,
            dto.County,
            dto.State,
            dto.Country,
            dto.PostalCode);
    }

    private async Task UpsertSalesPersonsAsync(
        SyncPersonDataDto personData,
        CancellationToken cancellationToken)
    {
        foreach (var assignment in personData.SalesAssignments)
        {
            await UpsertSalesPersonAsync(assignment.SalesPerson, cancellationToken);
        }
    }

    private static (string SalesPersonId, SalesAssignmentRole Role)[] MapSalesAssignments(
        SyncPersonDataDto personData)
    {
        return personData.SalesAssignments
            .Select(a => (a.SalesPerson.Id, a.Role))
            .ToArray();
    }

    private async Task UpsertSalesPersonAsync(
        SyncSalesPersonDto dto,
        CancellationToken cancellationToken)
    {
        var existing = await salesPersonRepository.GetByIdAsync(dto.Id, cancellationToken);

        if (existing is not null)
        {
            existing.Update(
                dto.Email,
                dto.Username,
                dto.FirstName,
                dto.LastName,
                dto.LotNumber,
                dto.FederatedId);
        }
        else
        {
            var salesPerson = SalesPerson.Assign(
                dto.Id,
                dto.Email,
                dto.Username,
                dto.FirstName,
                dto.LastName,
                dto.LotNumber,
                dto.FederatedId);

            salesPersonRepository.Add(salesPerson);
        }
    }
}
