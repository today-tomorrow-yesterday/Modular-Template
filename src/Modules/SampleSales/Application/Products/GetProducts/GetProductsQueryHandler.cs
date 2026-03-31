using Modules.SampleSales.Application.Products.GetProduct;
using Modules.SampleSales.Domain.Products;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.SampleSales.Application.Products.GetProducts;

internal sealed class GetProductsQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductsQuery, IReadOnlyCollection<ProductResponse>>
{
    public async Task<Result<IReadOnlyCollection<ProductResponse>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Product> products = await productRepository.GetAllAsync(
            request.Limit,
            cancellationToken);

        var response = products.Select(p => new ProductResponse(
            p.PublicId,
            p.Name,
            p.Description,
            p.Price.Amount,
            p.Price.Currency,
            p.IsActive,
            p.CreatedAtUtc,
            p.CreatedByUserId,
            p.ModifiedAtUtc,
            p.ModifiedByUserId)).ToList();

        return response;
    }
}
