using Modules.SampleSales.Domain.Catalogs;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.SampleSales.Application.Catalogs.GetCatalog;

internal sealed class GetCatalogQueryHandler(ICatalogRepository catalogRepository)
    : IQueryHandler<GetCatalogQuery, CatalogResponse>
{
    public async Task<Result<CatalogResponse>> Handle(
        GetCatalogQuery request,
        CancellationToken cancellationToken)
    {
        Catalog? catalog = await catalogRepository.GetByIdAsync(
            request.CatalogId,
            cancellationToken);

        if (catalog is null)
        {
            return Result.Failure<CatalogResponse>(CatalogErrors.NotFound(request.CatalogId));
        }

        return new CatalogResponse(
            catalog.PublicId,
            catalog.Name,
            catalog.Description,
            catalog.CreatedAtUtc,
            catalog.CreatedByUserId,
            catalog.ModifiedAtUtc,
            catalog.ModifiedByUserId);
    }
}
