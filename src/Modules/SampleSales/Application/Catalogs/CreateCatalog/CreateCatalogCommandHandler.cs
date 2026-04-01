using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Catalogs;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;

namespace Modules.SampleSales.Application.Catalogs.CreateCatalog;

internal sealed class CreateCatalogCommandHandler(
    ICatalogRepository catalogRepository,
    IUnitOfWork<ISampleSalesModule> unitOfWork)
    : ICommandHandler<CreateCatalogCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateCatalogCommand request,
        CancellationToken cancellationToken)
    {
        var catalogResult = Catalog.Create(request.Name, request.Description);

        if (catalogResult.IsFailure)
        {
            return Result.Failure<Guid>(catalogResult.Error);
        }

        catalogRepository.Add(catalogResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return catalogResult.Value.PublicId;
    }
}
