using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class CdcPricingQueries(SalesDbContext dbContext) : ICdcPricingQueries
{
    public async Task<CdcPricingHomeMultiplier?> GetActiveMultiplierForStateAsync(
        string stateCode,
        DateOnly? effectiveDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.CdcPricingHomeMultipliers
            .Where(m => m.IsActive && m.StateCode == stateCode);

        if (effectiveDate.HasValue)
        {
            query = query.Where(m => m.EffectiveDate <= effectiveDate.Value);
        }

        return await query
            .OrderByDescending(m => m.EffectiveDate)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
