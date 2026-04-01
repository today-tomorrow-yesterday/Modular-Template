using Modules.SampleSales.Domain.Products;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain.Results;

namespace Modules.SampleSales.Application.Products.GetProduct;

internal sealed class GetProductQueryHandler(IProductRepository productRepository)
    : IQueryHandler<GetProductQuery, ProductResponse>
{
    public async Task<Result<ProductResponse>> Handle(
        GetProductQuery request,
        CancellationToken cancellationToken)
    {
        Product? product = await productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound(request.ProductId));
        }

        return new ProductResponse(
            product.PublicId,
            product.Name,
            product.Description,
            product.Price.Amount,
            product.Price.Currency,
            product.IsActive,
            product.CreatedAtUtc,
            product.CreatedByUserId,
            product.ModifiedAtUtc,
            product.ModifiedByUserId);
    }
}
