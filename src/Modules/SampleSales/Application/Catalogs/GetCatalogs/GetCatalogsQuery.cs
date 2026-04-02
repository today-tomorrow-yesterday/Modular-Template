using Modules.SampleSales.Application.Catalogs.GetCatalog;
using ModularTemplate.Application.Messaging;

namespace Modules.SampleSales.Application.Catalogs.GetCatalogs;

public sealed record GetCatalogsQuery(int? Limit = 100, int Offset = 0) : IQuery<IReadOnlyCollection<CatalogResponse>>;
