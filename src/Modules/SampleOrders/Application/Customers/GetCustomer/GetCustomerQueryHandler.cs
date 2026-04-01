using Modules.SampleOrders.Domain.Customers;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Application.Customers.GetCustomer;

internal sealed class GetCustomerQueryHandler(ICustomerRepository customerRepository)
    : IQueryHandler<GetCustomerQuery, CustomerResponse>
{
    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerQuery request,
        CancellationToken cancellationToken)
    {
        Customer? customer = await customerRepository.GetByIdAsync(
            request.CustomerId,
            cancellationToken);

        if (customer is null)
        {
            return Result.Failure<CustomerResponse>(CustomerErrors.NotFound);
        }

        return new CustomerResponse(
            customer.PublicId,
            customer.Name.FirstName,
            customer.Name.MiddleName,
            customer.Name.LastName,
            customer.Name.FullName,
            customer.GetPrimaryEmail(),
            customer.Status.ToString(),
            customer.CreatedAtUtc,
            customer.CreatedByUserId,
            customer.ModifiedAtUtc,
            customer.ModifiedByUserId);
    }
}
