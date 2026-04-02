using ModularTemplate.Application.Messaging;

namespace Modules.SampleSales.Application.Products.GetProduct;

public sealed record GetProductQuery(Guid PublicProductId) : IQuery<ProductResponse>;
