using Microsoft.Extensions.Logging;
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

// Flow: SyncPartyFromCrmCommand → upsert SalesPersons (Person only) →
//   upsert Party (create or update) + sync contact points + sync identifiers →
//   raises PartyCreatedDomainEvent (create) or field-level change events (update)
internal sealed class SyncPartyFromCrmCommandHandler(
    IPartyRepository partyRepository,
    ISalesPersonRepository salesPersonRepository,
    IUnitOfWork<ICustomerModule> unitOfWork,
    ILogger<SyncPartyFromCrmCommandHandler> logger)
    : ICommandHandler<SyncPartyFromCrmCommand>
{
    public async Task<Result> Handle(
        SyncPartyFromCrmCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Upsert SalesPersons so FKs exist before Party insert/update
        await UpsertSalesPersonsAsync(request, cancellationToken);

        // Step 2: Create or update the Party aggregate (lookup by CRM party ID)
        var crmPartyIdValue = request.CrmPartyId.ToString();
        var existing = await partyRepository.GetForUpdateByIdentifierAsync(
            IdentifierType.CrmPartyId, crmPartyIdValue, cancellationToken);

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

        // Step 3: Persist all changes in a single transaction
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private void UpdateExistingParty(Party existing, SyncPartyFromCrmCommand request)
    {
        // Step 2a: Map mailing address
        MailingAddress? mailingAddress = null;
        if (request.MailingAddress is not null)
        {
            var a = request.MailingAddress;
            mailingAddress = MailingAddress.Create(
                a.AddressLine1, a.AddressLine2, a.City, a.County, a.State, a.Country, a.PostalCode);
        }

        // Step 2b: Update type-specific fields (TPH — existing entity is already the correct derived type)
        switch (existing)
        {
            case Person person when request.PersonData is not null:
                var assignments = request.PersonData.SalesAssignments
                    .Select(a => (a.SalesPerson.Id, a.Role))
                    .ToArray();
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

            default:
                logger.LogWarning(
                    "Party type mismatch during CRM sync: PartyId={PartyId}, ExistingType={ExistingType}, RequestedType={RequestedType}. Type-specific fields were not updated.",
                    request.CrmPartyId,
                    existing.GetType().Name,
                    request.PartyType);
                break;
        }

        // Step 2c: Replace contact points (CDC is full-state, not delta)
        existing.ReplaceContactPoints(
            request.ContactPoints.Select(cp => (cp.Type, cp.Value, cp.IsPrimary)));

        // Step 2d: Upsert identifiers by type
        foreach (var identifier in request.Identifiers)
        {
            existing.AddIdentifier(identifier.Type, identifier.Value);
        }
    }

    private static Result<Party> CreateNewParty(SyncPartyFromCrmCommand request)
    {
        // Step 2a: Map mailing address
        MailingAddress? mailingAddress = null;
        if (request.MailingAddress is not null)
        {
            var a = request.MailingAddress;
            mailingAddress = MailingAddress.Create(
                a.AddressLine1, a.AddressLine2, a.City, a.County, a.State, a.Country, a.PostalCode);
        }

        // Step 2b: Create Party via type-specific factory
        var personAssignments = request.PersonData is not null
            ? request.PersonData.SalesAssignments.Select(a => (a.SalesPerson.Id, a.Role)).ToArray()
            : Array.Empty<(string, SalesAssignmentRole)>();

        Party? party = request.PartyType switch
        {
            PartyType.Person when request.PersonData is not null => Person.SyncFromCrm(
                request.CrmPartyId,
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
                request.CrmPartyId,
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

        // Step 2c: Add contact points
        foreach (var cp in request.ContactPoints)
        {
            party.AddContactPoint(cp.Type, cp.Value, cp.IsPrimary);
        }

        // Step 2d: Add identifiers
        foreach (var id in request.Identifiers)
        {
            party.AddIdentifier(id.Type, id.Value);
        }

        return party;
    }

    private async Task UpsertSalesPersonsAsync(
        SyncPartyFromCrmCommand request,
        CancellationToken cancellationToken)
    {
        var hasPersonSalesData = request.PartyType == PartyType.Person && request.PersonData is not null;
        if (!hasPersonSalesData)
        {
            return;
        }

        foreach (var assignment in request.PersonData!.SalesAssignments)
        {
            var sp = assignment.SalesPerson;
            var existing = await salesPersonRepository.GetByIdAsync(sp.Id, cancellationToken); //TODO : Question, should we use the FederatedId?

            if (existing is not null)
            {
                existing.Update(sp.Email, sp.Username, sp.FirstName, sp.LastName, sp.LotNumber, sp.FederatedId);
            }
            else
            {
                salesPersonRepository.Add(
                    SalesPerson.Assign(sp.Id, sp.Email, sp.Username, sp.FirstName, sp.LastName, sp.LotNumber, sp.FederatedId));
            }
        }
    }
}
