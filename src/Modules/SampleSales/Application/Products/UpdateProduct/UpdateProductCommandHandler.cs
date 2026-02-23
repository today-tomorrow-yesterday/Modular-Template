using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Products;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.SampleSales.Application.Products.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork<ISampleSalesModule> unitOfWork)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound(request.ProductId));
        }

        var updateResult = product.Update(request.Name, request.Description, request.Price, request.IsActive);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
