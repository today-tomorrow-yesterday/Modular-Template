using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Application.Customers.CreateCustomer;

internal sealed class CreateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork<ISampleOrdersModule> unitOfWork)
    : ICommandHandler<CreateCustomerCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customerResult = Customer.Create(
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.Email,
            request.DateOfBirth);

        if (customerResult.IsFailure)
        {
            return Result.Failure<Guid>(customerResult.Error);
        }

        customerRepository.Add(customerResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return customerResult.Value.PublicId;
    }
}
