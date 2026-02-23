namespace Modules.Sales.Domain.Cdc;

public interface ICdcPricingQueries
{
    Task<CdcPricingHomeMultiplier?> GetActiveMultiplierForStateAsync(
        string stateCode,
        DateOnly? effectiveDate = null,
        CancellationToken cancellationToken = default);
}
