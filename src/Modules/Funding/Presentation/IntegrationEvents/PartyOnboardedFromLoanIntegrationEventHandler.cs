using Microsoft.Extensions.Logging;
using Modules.Customer.IntegrationEvents;
using Modules.Funding.Domain.CustomersCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Domain;

namespace Modules.Funding.Presentation.IntegrationEvents;

// Flow: Customer.PartyOnboardedFromLoanIntegrationEvent → Funding.UpsertCustomerCache → (TODO: Funding.MatchPendingFundingRequests)
internal sealed class PartyOnboardedFromLoanIntegrationEventHandler(
    ICacheWriteScope cacheWriteScope,
    ICustomerCacheWriter customerCacheWriter,
    IDateTimeProvider dateTimeProvider,
    ILogger<PartyOnboardedFromLoanIntegrationEventHandler> logger)
    : IntegrationEventHandler<PartyOnboardedFromLoanIntegrationEvent>
{
    public override async Task HandleAsync(
        PartyOnboardedFromLoanIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        using var _ = cacheWriteScope.AllowWrites();

        var loanId = integrationEvent.Identifiers
            .FirstOrDefault(i => i.Type == "LoanId")?.Value;

        logger.LogInformation(
            "Processing PartyOnboardedFromLoan: PartyId={PartyId}, LoanId={LoanId}",
            integrationEvent.PartyId,
            loanId);

        var customerCache = new CustomerCache
        {
            Id = integrationEvent.PartyId,
            LoanId = loanId,
            FirstName = integrationEvent.FirstName ?? string.Empty,
            LastName = integrationEvent.LastName ?? string.Empty,
            HomeCenterNumber = integrationEvent.HomeCenterNumber,
            LastSyncedAtUtc = dateTimeProvider.UtcNow
        };

        await customerCacheWriter.UpsertAsync(customerCache, cancellationToken);

        // TODO: When FundingRequest domain is implemented:
        // 1. Query PendingFundingRequests by LoanId
        // 2. If found: create FundingRequest with resolved PartyId, delete pending record
        // 3. Publish FundingRequestSubmitted integration event
    }
}

public interface ICustomerCacheWriter
{
    Task UpsertAsync(CustomerCache customerCache, CancellationToken cancellationToken = default);
    Task UpdateNameAsync(int partyId, string firstName, string lastName, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
    Task UpdateHomeCenterAsync(int partyId, int homeCenterNumber, DateTime lastSyncedAtUtc, CancellationToken cancellationToken = default);
}
