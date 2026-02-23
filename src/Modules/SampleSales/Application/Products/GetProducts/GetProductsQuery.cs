using Modules.SampleSales.Application.Products.GetProduct;
using Rtl.Core.Application.Messaging;

namespace Modules.SampleSales.Application.Products.GetProducts;

public sealed record GetProductsQuery(int? Limit = 100) : IQuery<IReadOnlyCollection<ProductResponse>>;
