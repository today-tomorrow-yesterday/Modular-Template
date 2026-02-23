using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Funding.IntegrationEvents;
using Modules.Sales.Application.FundingCache.UpsertFundingRequestCache;
using Modules.Sales.Domain.FundingCache;
using Rtl.Core.Application.EventBus;
using System.Text.Json;

namespace Modules.Sales.Presentation.IntegrationEvents.Funding;

// Flow: Funding.FundingRequestStatusChanged (EventBridge) → Send UpsertFundingRequestCacheCommand → Upsert cache.funding
// Updates display fields: status, approval dates, lender name. Same JSONB reconstitution as FundingRequestSubmitted.
internal sealed class FundingRequestStatusChangedIntegrationEventHandler(
    ISender sender,
    ILogger<FundingRequestStatusChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<FundingRequestStatusChangedIntegrationEvent>
{
    public async Task HandleAsync(
        FundingRequestStatusChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing FundingRequestStatusChanged: FundingRequestId={FundingRequestId}, Status={Status}, LenderId={LenderId}",
            integrationEvent.FundingRequestId,
            integrationEvent.FundingRequestStatus,
            integrationEvent.LenderId);

        var fundingKeys = BuildFundingKeys(integrationEvent.AppId);

        var cache = new FundingRequestCache
        {
            RefFundingRequestId = integrationEvent.FundingRequestId,
            SaleId = integrationEvent.SaleId,
            PackageId = integrationEvent.PackageId,
            FundingKeys = fundingKeys,
            LenderId = integrationEvent.LenderId,
            LenderName = integrationEvent.LenderName,
            Status = Enum.TryParse<FundingRequestStatus>(integrationEvent.FundingRequestStatus, out var s)
                ? s
                : FundingRequestStatus.Pending,
            ApprovalDate = integrationEvent.ApprovalDate,
            ApprovalExpirationDate = integrationEvent.ApprovalExpirationDate
        };

        await sender.Send(
            new UpsertFundingRequestCacheCommand(cache),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((FundingRequestStatusChangedIntegrationEvent)integrationEvent, cancellationToken);

    private static JsonDocument? BuildFundingKeys(int appId)
    {
        var keys = new List<object> { new { Key = "AppId", Value = appId.ToString() } };
        return JsonDocument.Parse(JsonSerializer.Serialize(keys));
    }
}
