using Rtl.Core.Domain;

namespace Modules.Funding.Domain.FundingRequests;

public interface IFundingRequestRepository : IRepository<FundingRequest, int>
{
    Task<IReadOnlyCollection<FundingRequest>> GetByRefCustomerPublicIdAndLoanIdAsync(
        Guid refCustomerPublicId,
        string loanId,
        CancellationToken cancellationToken = default);
}
