using Rtl.Core.Application.Messaging;

namespace Modules.SampleSales.Application.Catalogs.UpdateCatalog;

public sealed record UpdateCatalogCommand(
    int CatalogId,
    string Name,
    string? Description) : ICommand;
