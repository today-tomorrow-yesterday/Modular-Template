using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.SampleOrders.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork<ISampleOrdersModule> unitOfWork)
    : ICommandHandler<CreateCustomerCommand, int>
{
    public async Task<Result<int>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customerResult = Customer.Create(request.Name, request.Email);

        if (customerResult.IsFailure)
        {
            return Result.Failure<int>(customerResult.Error);
        }

        customerRepository.Add(customerResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customerResult.Value.Id;
    }
}
