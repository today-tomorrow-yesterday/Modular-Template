using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.SampleOrders.Application.Customers.AddContact;

internal sealed class AddContactCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork<ISampleOrdersModule> unitOfWork)
    : ICommandHandler<AddContactCommand>
{
    public async Task<Result> Handle(
        AddContactCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound);
        }

        customer.AddContact(request.Type, request.Value, request.IsPrimary);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
