using ModularTemplate.Application.Messaging;

namespace Modules.SampleSales.Application.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid PublicProductId,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive) : ICommand;
