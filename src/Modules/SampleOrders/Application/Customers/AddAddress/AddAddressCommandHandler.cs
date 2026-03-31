using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using Modules.SampleOrders.Domain.ValueObjects;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.SampleOrders.Application.Customers.AddAddress;

internal sealed class AddAddressCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork<ISampleOrdersModule> unitOfWork)
    : ICommandHandler<AddAddressCommand>
{
    public async Task<Result> Handle(
        AddAddressCommand request,
        CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound);
        }

        var address = Address.Create(
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var result = customer.AddAddress(address, request.IsPrimary);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
