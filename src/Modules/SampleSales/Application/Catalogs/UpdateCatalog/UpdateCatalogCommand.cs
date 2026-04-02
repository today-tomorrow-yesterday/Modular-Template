using ModularTemplate.Application.Messaging;

namespace Modules.SampleSales.Application.Catalogs.UpdateCatalog;

public sealed record UpdateCatalogCommand(
    Guid PublicCatalogId,
    string Name,
    string? Description) : ICommand;
