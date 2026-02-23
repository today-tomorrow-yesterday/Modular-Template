using Rtl.Core.Application.Messaging;

namespace Modules.SampleSales.Application.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    int ProductId,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive) : ICommand;
