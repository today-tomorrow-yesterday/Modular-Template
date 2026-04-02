using ModularTemplate.Application.Messaging;

namespace Modules.SampleSales.Application.Catalogs.GetCatalog;

public sealed record GetCatalogQuery(Guid PublicCatalogId) : IQuery<CatalogResponse>;
