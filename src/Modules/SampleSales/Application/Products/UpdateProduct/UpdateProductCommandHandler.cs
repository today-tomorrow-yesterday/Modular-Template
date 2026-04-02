using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Products;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Results;

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
        var product = await productRepository.GetByPublicIdAsync(request.PublicProductId, cancellationToken);

        var updateResult = product.Update(request.Name, request.Description, request.Price, request.IsActive);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
