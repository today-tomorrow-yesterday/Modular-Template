using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Funding.Domain.FundingRequests;

public sealed class PendingFundingRequest : Entity
{
    private readonly List<FundingKey> _fundingKeys = [];

    private PendingFundingRequest() {}

    [SensitiveData] public string LoanId { get; private set; } = string.Empty;

    public int SaleId { get; private set; }

    public int PackageId { get; private set; }

    public Guid? RefCustomerPublicId { get; private set; }

    [SensitiveData] public decimal RequestAmount { get; private set; }

    public int? HomeCenterNumber { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<FundingKey> FundingKeys => _fundingKeys.AsReadOnly();

    public static PendingFundingRequest Create(
        string loanId,
        int saleId,
        int packageId,
        Guid? refCustomerPublicId,
        decimal requestAmount,
        int? homeCenterNumber,
        IEnumerable<FundingKey> fundingKeys)
    {
        var pending = new PendingFundingRequest
        {
            LoanId = loanId,
            SaleId = saleId,
            PackageId = packageId,
            RefCustomerPublicId = refCustomerPublicId,
            RequestAmount = requestAmount,
            HomeCenterNumber = homeCenterNumber,
            CreatedAtUtc = DateTime.UtcNow
        };

        pending._fundingKeys.AddRange(fundingKeys);

        return pending;
    }
}
