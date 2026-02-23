namespace Modules.SampleSales.Application.Catalogs.GetCatalog;

public sealed record CatalogResponse(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    Guid CreatedByUserId,
    DateTime? ModifiedAtUtc,
    Guid? ModifiedByUserId);
