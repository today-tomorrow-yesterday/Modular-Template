using Rtl.Core.Application.Messaging;

namespace Modules.SampleSales.Application.Catalogs.CreateCatalog;

public sealed record CreateCatalogCommand(
    string Name,
    string? Description) : ICommand<Guid>;
