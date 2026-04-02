using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using Modules.SampleOrders.Domain.ValueObjects;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;

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
        var customer = await customerRepository.GetByPublicIdAsync(request.PublicCustomerId, cancellationToken);

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
