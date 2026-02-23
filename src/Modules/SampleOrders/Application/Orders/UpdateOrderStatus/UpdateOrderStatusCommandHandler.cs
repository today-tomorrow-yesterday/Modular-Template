using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Orders;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

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
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound(request.OrderId));
        }

        var updateResult = order.UpdateStatus(request.NewStatus);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
