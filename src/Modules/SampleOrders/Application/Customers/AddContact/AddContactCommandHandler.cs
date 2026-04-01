using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;

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
