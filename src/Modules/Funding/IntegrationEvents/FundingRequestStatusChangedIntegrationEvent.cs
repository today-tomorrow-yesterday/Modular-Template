using Rtl.Core.Application.EventBus;

namespace Modules.Funding.IntegrationEvents;

// ECST event — status/approval updates (Pending→Approved→Funded).
// Consumer: Sales (cache.funding)
public sealed record FundingRequestStatusChangedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int FundingRequestId,
    int SaleId,
    int PackageId,
    string FundingRequestStatus,
    int AppId,
    DateTimeOffset? ApprovalDate,
    DateTimeOffset? ApprovalExpirationDate,
    int LenderId,
    string? LenderName) : IntegrationEvent(Id, OccurredOnUtc);
