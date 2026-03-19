using Modules.Sales.Domain;
using Modules.Sales.Domain.CustomersCache;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Sales.CreateSale;

internal sealed class CreateSaleCommandHandler(
    ICustomerCacheRepository customerCacheRepository,
    IRetailLocationRepository retailLocationRepository,
    ISaleRepository saleRepository,
    ISaleNumberGenerator saleNumberGenerator,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<CreateSaleCommand, CreateSaleResult>
{
    public async Task<Result<CreateSaleResult>> Handle(
        CreateSaleCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Resolve customer from cache by PublicId
        var customerResult = await ResolveCustomerAsync(request.CustomerPublicId, cancellationToken);
        if (customerResult.IsFailure)
        {
            return Result.Failure<CreateSaleResult>(customerResult.Error);
        }

        // Step 1b: Idempotency — return existing sale if one already exists for this customer
        var existingSale = await saleRepository.GetByCustomerIdAsync(customerResult.Value.Id, cancellationToken);
        if (existingSale is not null)
        {
            return new CreateSaleResult(existingSale.PublicId, existingSale.SaleNumber);
        }

        // Step 2: Resolve retail location and validate it is active
        var locationResult = await ResolveActiveRetailLocationAsync(request.HomeCenterNumber, cancellationToken);
        if (locationResult.IsFailure)
        {
            return Result.Failure<CreateSaleResult>(locationResult.Error);
        }

        // Step 3: Generate next sale number
        var saleNumber = await saleNumberGenerator.GenerateNextAsync(cancellationToken);

        // Step 4: Create sale aggregate (raises SaleSummaryChangedDomainEvent)
        var sale = Sale.Create(customerResult.Value.Id, locationResult.Value.Id, request.SaleType, saleNumber);

        // Step 5: Persist
        saleRepository.Add(sale);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 6: Return created sale identifiers
        return new CreateSaleResult(sale.PublicId, sale.SaleNumber);
    }

    private async Task<Result<CustomerCache>> ResolveCustomerAsync(
        Guid customerPublicId, CancellationToken cancellationToken)
    {
        var customer = await customerCacheRepository.GetByRefPublicIdAsync(customerPublicId, cancellationToken);

        return customer is not null
            ? customer
            : Result.Failure<CustomerCache>(SaleErrors.CustomerNotFound(customerPublicId));
    }

    private async Task<Result<RetailLocation>> ResolveActiveRetailLocationAsync(
        int homeCenterNumber, CancellationToken cancellationToken)
    {
        var retailLocation = await retailLocationRepository.GetByHomeCenterNumberAsync(
            homeCenterNumber, cancellationToken);

        if (retailLocation is null)
        {
            return Result.Failure<RetailLocation>(SaleErrors.RetailLocationNotFound(homeCenterNumber));
        }

        if (!retailLocation.IsActive)
        {
            return Result.Failure<RetailLocation>(SaleErrors.RetailLocationInactive(homeCenterNumber));
        }

        return retailLocation;
    }
}
