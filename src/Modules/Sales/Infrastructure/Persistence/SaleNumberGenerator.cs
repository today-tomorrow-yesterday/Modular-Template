using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Sales;

namespace Modules.Sales.Infrastructure.Persistence;

internal sealed class SaleNumberGenerator(SalesDbContext dbContext) : ISaleNumberGenerator
{
    // Uses a PostgreSQL sequence for atomic, race-condition-free sale number generation.
    // Sequence created via migration: CREATE SEQUENCE IF NOT EXISTS sales.sale_number_seq START WITH 100001;
    public async Task<int> GenerateNextAsync(CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Database
            .SqlQueryRaw<int>("SELECT nextval('sales.sale_number_seq') AS \"Value\"")
            .SingleAsync(cancellationToken);

        return result;
    }
}
