namespace Modules.SampleSales.Application.Products.GetProduct;

public sealed record ProductResponse(
    Guid PublicId,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    bool IsActive,
    DateTime CreatedAtUtc,
    Guid CreatedByUserId,
    DateTime? ModifiedAtUtc,
    Guid? ModifiedByUserId);
