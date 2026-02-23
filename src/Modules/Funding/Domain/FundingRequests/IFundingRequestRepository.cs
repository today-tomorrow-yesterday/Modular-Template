using Rtl.Core.Domain;

namespace Modules.Funding.Domain.FundingRequests;

public interface IFundingRequestRepository : IRepository<FundingRequest, int>
{
    Task<IReadOnlyCollection<FundingRequest>> GetByRefCustomerIdAndLoanIdAsync(
        int refCustomerId,
        string loanId,
        CancellationToken cancellationToken = default);
}
