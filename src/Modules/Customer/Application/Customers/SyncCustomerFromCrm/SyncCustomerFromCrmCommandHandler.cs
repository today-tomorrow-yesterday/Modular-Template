using Modules.Customer.Domain;
using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.SalesPersons;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Application.Customers.SyncCustomerFromCrm;

internal sealed class SyncCustomerFromCrmCommandHandler(
    ICustomerRepository customerRepository,
    ISalesPersonRepository salesPersonRepository,
    IUnitOfWork<ICustomerModule> unitOfWork)
    : ICommandHandler<SyncCustomerFromCrmCommand>
{
    public async Task<Result> Handle(
        SyncCustomerFromCrmCommand request,
        CancellationToken cancellationToken)
    {
        await UpsertSalesPersonsAsync(request, cancellationToken);

        var crmCustomerIdValue = request.CrmCustomerId.ToString();
        var existing = await customerRepository.GetForUpdateByIdentifierAsync(
            IdentifierType.CrmCustomerId, crmCustomerIdValue, cancellationToken);

        if (existing is not null)
        {
            UpdateExistingCustomer(existing, request);
        }
        else
        {
            var newCustomer = CreateNewCustomer(request);
            customerRepository.Add(newCustomer);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static void UpdateExistingCustomer(Domain.Customers.Entities.Customer existing, SyncCustomerFromCrmCommand request)
    {
        MailingAddress? mailingAddress = null;
        if (request.MailingAddress is not null)
        {
            var a = request.MailingAddress;
            mailingAddress = MailingAddress.Create(
                a.AddressLine1, a.AddressLine2, a.City, a.County, a.State, a.Country, a.PostalCode);
        }

        var assignments = request.SalesAssignments
            .Select(a => (a.SalesPerson.Id, a.Role))
            .ToArray();
        existing.UpdateFromCrmSync(
            request.FirstName is not null
                ? CustomerName.Create(request.FirstName, request.MiddleName, request.LastName, request.NameExtension)
                : null,
            request.DateOfBirth,
            assignments,
            request.HomeCenterNumber,
            request.SalesforceUrl,
            mailingAddress,
            request.LastModifiedOn);

        existing.ReplaceContactPoints(
            request.ContactPoints.Select(cp => (cp.Type, cp.Value, cp.IsPrimary)));

        foreach (var identifier in request.Identifiers)
        {
            existing.AddIdentifier(identifier.Type, identifier.Value);
        }
    }

    private static Domain.Customers.Entities.Customer CreateNewCustomer(SyncCustomerFromCrmCommand request)
    {
        MailingAddress? mailingAddress = null;
        if (request.MailingAddress is not null)
        {
            var a = request.MailingAddress;
            mailingAddress = MailingAddress.Create(
                a.AddressLine1, a.AddressLine2, a.City, a.County, a.State, a.Country, a.PostalCode);
        }

        var assignments = request.SalesAssignments
            .Select(a => (a.SalesPerson.Id, a.Role))
            .ToArray();

        var customer = Domain.Customers.Entities.Customer.SyncFromCrm(
            request.CrmCustomerId,
            request.HomeCenterNumber,
            request.LifecycleStage,
            request.FirstName is not null
                ? CustomerName.Create(request.FirstName, request.MiddleName, request.LastName, request.NameExtension)
                : null,
            request.DateOfBirth,
            assignments,
            request.SalesforceUrl,
            mailingAddress,
            request.CreatedOn,
            request.LastModifiedOn);

        foreach (var cp in request.ContactPoints)
        {
            customer.AddContactPoint(cp.Type, cp.Value, cp.IsPrimary);
        }

        foreach (var id in request.Identifiers)
        {
            customer.AddIdentifier(id.Type, id.Value);
        }

        return customer;
    }

    private async Task UpsertSalesPersonsAsync(
        SyncCustomerFromCrmCommand request,
        CancellationToken cancellationToken)
    {
        foreach (var assignment in request.SalesAssignments)
        {
            var sp = assignment.SalesPerson;
            var existing = await salesPersonRepository.GetByIdAsync(sp.Id, cancellationToken);

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
