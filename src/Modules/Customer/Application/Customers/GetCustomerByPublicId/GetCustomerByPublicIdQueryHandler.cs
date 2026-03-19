using Modules.Customer.Domain.Customers;
using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Errors;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Application.Customers.GetCustomerByPublicId;

internal sealed class GetCustomerByPublicIdQueryHandler(
    ICustomerRepository customerRepository)
    : IQueryHandler<GetCustomerByPublicIdQuery, CustomerResponse>
{
    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerByPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByPublicIdAsync(
            request.PublicId,
            cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(
                CustomerErrors.NotFoundByPublicId(request.PublicId));
        }

        var coBuyer = customer.CoBuyer;

        return new CustomerResponse(
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
            })],
            coBuyer?.PublicId,
            coBuyer?.Name?.FirstName,
            coBuyer?.Name?.MiddleName,
            coBuyer?.Name?.LastName,
            coBuyer?.DateOfBirth,
            [.. customer.ContactPoints.Select(cp => new ContactPointResponse(cp.Type.ToString(), cp.Value, cp.IsPrimary))],
            [.. customer.Identifiers.Select(id => new IdentifierResponse(id.Type.ToString(), id.Value))],
            customer.MailingAddress is { } a
                ? new MailingAddressResponse(a.AddressLine1, a.AddressLine2, a.City, a.County, a.State, a.Country, a.PostalCode)
                : null,
            customer.SalesforceUrl,
            customer.LastSyncedAtUtc);
    }
}
