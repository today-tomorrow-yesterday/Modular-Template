using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Catalogs;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;

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
        var catalog = await catalogRepository.GetByPublicIdAsync(request.PublicCatalogId, cancellationToken);

        var updateResult = catalog.Update(request.Name, request.Description);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
