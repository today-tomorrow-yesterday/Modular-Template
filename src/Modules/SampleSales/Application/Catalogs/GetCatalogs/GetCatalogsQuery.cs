using Modules.SampleSales.Application.Catalogs.GetCatalog;
using Rtl.Core.Application.Messaging;

namespace Modules.SampleSales.Application.Catalogs.GetCatalogs;

public sealed record GetCatalogsQuery(int? Limit = 100) : IQuery<IReadOnlyCollection<CatalogResponse>>;
