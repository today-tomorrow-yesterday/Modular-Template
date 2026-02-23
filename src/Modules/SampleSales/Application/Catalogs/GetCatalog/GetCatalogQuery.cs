using Rtl.Core.Application.Messaging;

namespace Modules.SampleSales.Application.Catalogs.GetCatalog;

public sealed record GetCatalogQuery(int CatalogId) : IQuery<CatalogResponse>;
