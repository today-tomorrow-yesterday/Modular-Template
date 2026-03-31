using Modules.SampleOrders.Application.Customers.GetCustomer;
using Modules.SampleOrders.Domain.Customers;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.SampleOrders.Application.Customers.GetCustomers;

internal sealed class GetCustomersQueryHandler(ICustomerRepository customerRepository)
    : IQueryHandler<GetCustomersQuery, IReadOnlyCollection<CustomerResponse>>
{
    public async Task<Result<IReadOnlyCollection<CustomerResponse>>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Customer> customers = await customerRepository.GetAllAsync(
            request.Limit,
            cancellationToken);

        var response = customers.Select(c => new CustomerResponse(
            c.PublicId,
            c.Name.FirstName,
            c.Name.MiddleName,
            c.Name.LastName,
            c.Name.FullName,
            c.GetPrimaryEmail(),
            c.Status.ToString(),
            c.CreatedAtUtc,
            c.CreatedByUserId,
            c.ModifiedAtUtc,
            c.ModifiedByUserId)).ToList();

        return response;
    }
}
