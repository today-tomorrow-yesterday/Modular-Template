using Modules.Funding.Domain.CustomersCache;
using Modules.Funding.Domain.Enums;
using Modules.Funding.Domain.FundingRequests.Events;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Funding.Domain.FundingRequests;

public sealed class FundingRequest : AuditableEntity
{
    private readonly List<FundingKey> _fundingKeys = [];

    private FundingRequest() {}

    public int SaleId { get; private set; }

    public int PackageId { get; private set; }

    public int? CustomerId { get; private set; }
    public CustomerCache? Customer { get; private set; }

    public Guid? RefCustomerPublicId { get; private set; }

    public FundingRequestType RequestType { get; private set; }

    public FundingRequestStatus Status { get; private set; }

    [SensitiveData] public decimal RequestAmount { get; private set; }

    public DateTimeOffset? ApprovalDate { get; private set; }

    public DateTimeOffset? ApprovalExpirationDate { get; private set; }

    public int LenderId { get; private set; }

    [SensitiveData] public string? LenderName { get; private set; }

    public int? HomeCenterNumber { get; private set; }

    public IReadOnlyCollection<FundingKey> FundingKeys => _fundingKeys.AsReadOnly();

    public static FundingRequest Create(
        int saleId,
        int packageId,
        int? customerId,
        Guid? refCustomerPublicId,
        FundingRequestType requestType,
        decimal requestAmount,
        int? homeCenterNumber,
        IEnumerable<FundingKey> fundingKeys)
    {
        var request = new FundingRequest
        {
            SaleId = saleId,
            PackageId = packageId,
            CustomerId = customerId,
            RefCustomerPublicId = refCustomerPublicId,
            RequestType = requestType,
            Status = FundingRequestStatus.Pending,
            RequestAmount = requestAmount,
            LenderId = 0,
            HomeCenterNumber = homeCenterNumber
        };

        request._fundingKeys.AddRange(fundingKeys);

        request.Raise(new FundingRequestSubmittedDomainEvent());

        return request;
    }

    public void UpdateStatus(FundingRequestStatus status)
    {
        Status = status;
        Raise(new FundingRequestStatusChangedDomainEvent());
    }

    public void SetApproval(DateTimeOffset approvalDate, DateTimeOffset expirationDate, int lenderId, string lenderName)
    {
        ApprovalDate = approvalDate;
        ApprovalExpirationDate = expirationDate;
        LenderId = lenderId;
        LenderName = lenderName;
        Status = FundingRequestStatus.Approved;
        Raise(new FundingRequestStatusChangedDomainEvent());
    }

    public string? GetFundingKeyValue(string key) =>
        _fundingKeys.FirstOrDefault(fk => fk.Key == key)?.Value;
}
