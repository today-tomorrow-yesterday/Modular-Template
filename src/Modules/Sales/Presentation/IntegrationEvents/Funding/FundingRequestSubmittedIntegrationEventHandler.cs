using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Funding.IntegrationEvents;
using Modules.Sales.Application.FundingCache.UpsertFundingRequestCache;
using Modules.Sales.Domain.FundingCache;
using Rtl.Core.Application.EventBus;
using System.Text.Json;

namespace Modules.Sales.Presentation.IntegrationEvents.Funding;

// Flow: Funding.FundingRequestSubmitted (EventBridge) → Send UpsertFundingRequestCacheCommand → Upsert cache.funding
// Reconstitutes flat AppId/LoanId fields into funding_keys JSONB column.
// Sales domain code never accesses lender-specific identifiers directly — only iSeries adapter extracts from funding_keys.
internal sealed class FundingRequestSubmittedIntegrationEventHandler(
    ISender sender,
    ILogger<FundingRequestSubmittedIntegrationEventHandler> logger)
    : IntegrationEventHandler<FundingRequestSubmittedIntegrationEvent>
{
    public override async Task HandleAsync(
        FundingRequestSubmittedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing FundingRequestSubmitted: FundingRequestId={FundingRequestId}, SaleId={SaleId}, PackageId={PackageId}, Status={Status}",
            integrationEvent.FundingRequestId,
            integrationEvent.SaleId,
            integrationEvent.PackageId,
            integrationEvent.FundingRequestStatus);

        var fundingKeys = BuildFundingKeys(integrationEvent.AppId, integrationEvent.LoanId);

        var cache = new FundingRequestCache
        {
            RefFundingRequestId = integrationEvent.FundingRequestId,
            SaleId = integrationEvent.SaleId,
            PackageId = integrationEvent.PackageId,
            FundingKeys = fundingKeys,
            LenderId = 0,
            LenderName = null,
            Status = Enum.TryParse<FundingRequestStatus>(integrationEvent.FundingRequestStatus, out var s)
                ? s
                : FundingRequestStatus.Pending,
            ApprovalDate = null,
            ApprovalExpirationDate = null
        };

        await sender.Send(
            new UpsertFundingRequestCacheCommand(cache),
            cancellationToken);
    }

    private static JsonDocument? BuildFundingKeys(int appId, string? loanId)
    {
        var keys = new List<object> { new { Key = "AppId", Value = appId.ToString() } };

        if (!string.IsNullOrEmpty(loanId))
            keys.Add(new { Key = "LoanId", Value = loanId });

        return JsonDocument.Parse(JsonSerializer.Serialize(keys));
    }
}
