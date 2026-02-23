using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.SampleOrders.Application.Customers.UpdateCustomer;

internal sealed class UpdateCustomerCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork<ISampleOrdersModule> unitOfWork)
    : ICommandHandler<UpdateCustomerCommand>
{
    public async Task<Result> Handle(
        UpdateCustomerCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(request.CustomerId));
        }

        var updateResult = customer.Update(request.Name, request.Email);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
