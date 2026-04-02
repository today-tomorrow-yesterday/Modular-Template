using Modules.SampleSales.Domain.Catalogs;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain.Results;

namespace Modules.SampleSales.Application.Catalogs.GetCatalog;

internal sealed class GetCatalogQueryHandler(ICatalogRepository catalogRepository)
    : IQueryHandler<GetCatalogQuery, CatalogResponse>
{
    public async Task<Result<CatalogResponse>> Handle(
        GetCatalogQuery request,
        CancellationToken cancellationToken)
    {
        Catalog catalog = await catalogRepository.GetByPublicIdAsync(
            request.PublicCatalogId,
            cancellationToken);

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
