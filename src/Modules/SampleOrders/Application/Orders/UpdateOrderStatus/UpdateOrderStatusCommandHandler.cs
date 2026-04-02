using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Orders;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Application.Orders.UpdateOrderStatus;

internal sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork<ISampleOrdersModule> unitOfWork)
    : ICommandHandler<UpdateOrderStatusCommand>
{
    public async Task<Result> Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByPublicIdAsync(request.PublicOrderId, cancellationToken);

        var updateResult = order.UpdateStatus(request.NewStatus);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
