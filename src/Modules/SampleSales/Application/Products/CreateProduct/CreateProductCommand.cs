using Rtl.Core.Application.Messaging;

namespace Modules.SampleSales.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    decimal? InternalCost) : ICommand<Guid>;
