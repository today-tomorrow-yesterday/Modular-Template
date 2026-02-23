using Modules.Sales.Domain;
using Modules.Sales.Domain.PartiesCache;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Sales.CreateSale;

internal sealed class CreateSaleCommandHandler(
    IPartyCacheRepository partyCacheRepository,
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
        // Step 1: Resolve party from cache by PublicId
        var partyResult = await ResolvePartyAsync(request.PartyPublicId, cancellationToken);
        if (partyResult.IsFailure)
        {
            return Result.Failure<CreateSaleResult>(partyResult.Error);
        }

        // Step 1b: Idempotency — return existing sale if one already exists for this party
        var existingSale = await saleRepository.GetByPartyIdAsync(partyResult.Value.Id, cancellationToken);
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
        var sale = Sale.Create(partyResult.Value.Id, locationResult.Value.Id, request.SaleType, saleNumber);

        // Step 5: Persist
        saleRepository.Add(sale);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 6: Return created sale identifiers
        return new CreateSaleResult(sale.PublicId, sale.SaleNumber);
    }

    private async Task<Result<PartyCache>> ResolvePartyAsync(
        Guid partyPublicId, CancellationToken cancellationToken)
    {
        var party = await partyCacheRepository.GetByRefPublicIdAsync(partyPublicId, cancellationToken);

        return party is not null
            ? party
            : Result.Failure<PartyCache>(SaleErrors.PartyNotFound(partyPublicId));
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
