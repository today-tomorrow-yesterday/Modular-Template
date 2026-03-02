using Microsoft.EntityFrameworkCore;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Funding.Infrastructure.Persistence;

public sealed class FundingDbContextFactory : DesignTimeDbContextFactoryBase<FundingDbContext>
{
    protected override FundingDbContext CreateContext(DbContextOptions<FundingDbContext> options) => new(options);
}
