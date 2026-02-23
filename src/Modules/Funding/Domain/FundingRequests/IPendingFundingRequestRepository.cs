using Rtl.Core.Domain;

namespace Modules.Funding.Domain.FundingRequests;

public interface IPendingFundingRequestRepository : IRepository<PendingFundingRequest, int>
{
    Task<PendingFundingRequest?> GetByLoanIdAsync(string loanId, CancellationToken cancellationToken = default);
}
