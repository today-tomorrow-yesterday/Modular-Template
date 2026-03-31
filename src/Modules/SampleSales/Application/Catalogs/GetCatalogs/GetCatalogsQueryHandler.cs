using Modules.SampleSales.Application.Catalogs.GetCatalog;
using Modules.SampleSales.Domain.Catalogs;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.SampleSales.Application.Catalogs.GetCatalogs;

internal sealed class GetCatalogsQueryHandler(ICatalogRepository catalogRepository)
    : IQueryHandler<GetCatalogsQuery, IReadOnlyCollection<CatalogResponse>>
{
    public async Task<Result<IReadOnlyCollection<CatalogResponse>>> Handle(
        GetCatalogsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Catalog> catalogs = await catalogRepository.GetAllAsync(
            request.Limit,
            cancellationToken);

        var response = catalogs.Select(c => new CatalogResponse(
            c.PublicId,
            c.Name,
            c.Description,
            c.CreatedAtUtc,
            c.CreatedByUserId,
            c.ModifiedAtUtc,
            c.ModifiedByUserId)).ToList();

        return response;
    }
}
