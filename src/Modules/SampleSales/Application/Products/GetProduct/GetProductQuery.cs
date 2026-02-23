using Rtl.Core.Application.Messaging;

namespace Modules.SampleSales.Application.Products.GetProduct;

public sealed record GetProductQuery(int ProductId) : IQuery<ProductResponse>;
