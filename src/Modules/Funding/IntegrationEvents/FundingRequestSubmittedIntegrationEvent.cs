using Rtl.Core.Application.EventBus;

namespace Modules.Funding.IntegrationEvents;

// ECST event — initial funding data with FundingKeys (lender identifiers for iSeries adapter).
// Consumer: Sales (cache.funding)
public sealed record FundingRequestSubmittedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int FundingRequestId,
    int SaleId,
    int PackageId,
    int AppId,
    string? LoanId,
    string FundingRequestStatus) : IntegrationEvent(Id, OccurredOnUtc);
