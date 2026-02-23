using Rtl.Core.Application.EventBus;

namespace Modules.Sales.IntegrationEvents;

// Published to Funding module to trigger FundingRequest creation.
public sealed record PackageReadyForFundingIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    int SaleId,
    int PackageId,
    int? CustomerId,
    int RequestTypeId,
    decimal RequestAmount,
    int? HomeCenterNumber,
    int LenderId,
    string? LenderName,
    string? StockNumber,
    List<FundingKeyDto> FundingKeys) : IntegrationEvent(Id, OccurredOnUtc);

public sealed record FundingKeyDto(string Key, string Value);
