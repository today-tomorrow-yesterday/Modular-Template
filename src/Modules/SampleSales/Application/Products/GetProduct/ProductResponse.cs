namespace Modules.SampleSales.Application.Products.GetProduct;

public sealed record ProductResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive,
    DateTime CreatedAtUtc,
    Guid CreatedByUserId,
    DateTime? ModifiedAtUtc,
    Guid? ModifiedByUserId);
