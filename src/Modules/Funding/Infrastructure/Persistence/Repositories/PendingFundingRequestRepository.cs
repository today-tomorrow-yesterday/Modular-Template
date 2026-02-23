using Microsoft.EntityFrameworkCore;
using Modules.Funding.Domain.FundingRequests;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Funding.Infrastructure.Persistence.Repositories;

internal sealed class PendingFundingRequestRepository(FundingDbContext dbContext)
    : Repository<PendingFundingRequest, int, FundingDbContext>(dbContext), IPendingFundingRequestRepository
{
    protected override Expression<Func<PendingFundingRequest, int>> IdSelector => entity => entity.Id;

    public async Task<PendingFundingRequest?> GetByLoanIdAsync(
        string loanId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.LoanId == loanId, cancellationToken);
    }
}
