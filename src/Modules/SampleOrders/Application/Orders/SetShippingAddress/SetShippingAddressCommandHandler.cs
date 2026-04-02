using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.ValueObjects;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Application.Orders.SetShippingAddress;

internal sealed class SetShippingAddressCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork<ISampleOrdersModule> unitOfWork)
    : ICommandHandler<SetShippingAddressCommand>
{
    public async Task<Result> Handle(
        SetShippingAddressCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByPublicIdAsync(request.PublicOrderId, cancellationToken);

        var address = Address.Create(
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.State,
            request.PostalCode,
            request.Country);

        var result = order.SetShippingAddress(address);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
