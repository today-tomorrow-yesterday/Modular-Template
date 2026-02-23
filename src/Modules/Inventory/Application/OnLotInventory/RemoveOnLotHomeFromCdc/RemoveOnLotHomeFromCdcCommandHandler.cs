using Modules.Inventory.Domain;
using Modules.Inventory.Domain.OnLotHomes;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.OnLotInventory.RemoveOnLotHomeFromCdc;

// Flow: CDC feed delete → RemoveOnLotHomeFromCdcCommand → MarkRemoved + Remove
internal sealed class RemoveOnLotHomeFromCdcCommandHandler(
    IOnLotHomeRepository repository,
    IUnitOfWork<IInventoryModule> unitOfWork) : ICommandHandler<RemoveOnLotHomeFromCdcCommand>
{
    public async Task<Result> Handle(
        RemoveOnLotHomeFromCdcCommand request,
        CancellationToken cancellationToken)
    {
        var home = await repository.GetByIdAsync(request.OnLotHomeId, cancellationToken);

        if (home is null)
        {
            return Result.Success();
        }

        home.MarkRemoved();
        repository.Remove(home);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
