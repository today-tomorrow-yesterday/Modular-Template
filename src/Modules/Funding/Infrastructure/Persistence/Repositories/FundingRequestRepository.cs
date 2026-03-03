using Microsoft.EntityFrameworkCore;
using Modules.Funding.Domain.FundingRequests;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Funding.Infrastructure.Persistence.Repositories;

internal sealed class FundingRequestRepository(FundingDbContext dbContext)
    : Repository<FundingRequest, int, FundingDbContext>(dbContext), IFundingRequestRepository
{
    protected override Expression<Func<FundingRequest, int>> IdSelector => entity => entity.Id;

    public async Task<IReadOnlyCollection<FundingRequest>> GetByRefCustomerPublicIdAndLoanIdAsync(
        Guid refCustomerPublicId,
        string loanId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(f => f.RefCustomerPublicId == refCustomerPublicId)
            .ToListAsync(cancellationToken);
    }
}
