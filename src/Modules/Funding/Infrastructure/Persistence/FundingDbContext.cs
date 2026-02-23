using Microsoft.EntityFrameworkCore;
using Modules.Funding.Domain;
using Modules.Funding.Domain.CustomersCache;
using Modules.Funding.Domain.FundingRequests;
using Modules.Funding.Infrastructure.Persistence.Configurations;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Funding.Infrastructure.Persistence;

public sealed class FundingDbContext(DbContextOptions<FundingDbContext> options)
    : ModuleDbContext<FundingDbContext>(options), IUnitOfWork<IFundingModule>
{
    protected override string Schema => Schemas.Fundings;

    internal DbSet<CustomerCache> CustomersCache => Set<CustomerCache>();
    internal DbSet<FundingRequest> FundingRequests => Set<FundingRequest>();
    internal DbSet<PendingFundingRequest> PendingFundingRequests => Set<PendingFundingRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new CustomerCacheConfiguration());
        modelBuilder.ApplyConfiguration(new FundingRequestConfiguration());
        modelBuilder.ApplyConfiguration(new PendingFundingRequestConfiguration());
    }
}
