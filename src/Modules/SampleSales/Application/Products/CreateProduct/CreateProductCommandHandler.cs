using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Products;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.SampleSales.Application.Products.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork<ISampleSalesModule> unitOfWork)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var productResult = Product.Create(request.Name, request.Description, request.Price, request.InternalCost);

        if (productResult.IsFailure)
        {
            return Result.Failure<Guid>(productResult.Error);
        }

        productRepository.Add(productResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return productResult.Value.PublicId;
    }
}
