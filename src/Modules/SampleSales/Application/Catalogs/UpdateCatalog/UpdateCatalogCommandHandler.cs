using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Catalogs;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.SampleSales.Application.Catalogs.UpdateCatalog;

internal sealed class UpdateCatalogCommandHandler(
    ICatalogRepository catalogRepository,
    IUnitOfWork<ISampleSalesModule> unitOfWork)
    : ICommandHandler<UpdateCatalogCommand>
{
    public async Task<Result> Handle(
        UpdateCatalogCommand request,
        CancellationToken cancellationToken)
    {
        var catalog = await catalogRepository.GetByIdAsync(request.CatalogId, cancellationToken);

        if (catalog is null)
        {
            return Result.Failure(CatalogErrors.NotFound(request.CatalogId));
        }

        var updateResult = catalog.Update(request.Name, request.Description);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
