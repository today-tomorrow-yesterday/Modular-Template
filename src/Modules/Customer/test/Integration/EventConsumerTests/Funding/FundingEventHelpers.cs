using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.EventConsumerTests.Abstractions;
using Modules.Funding.IntegrationEvents;
using Rtl.Core.Application.EventBus;

namespace Modules.Customer.EventConsumerTests.Funding;

/// <summary>
/// Helpers for triggering Funding module integration events in Customer integration tests.
///
/// The OnboardCustomerFromLoanRequestedIntegrationEvent is published by Funding
/// and consumed by Customer's OnboardCustomerFromLoanRequestedIntegrationEventHandler,
/// which sends OnboardCustomerFromLoanCommand to create the customer.
///
/// Since the InMemoryEventBus dispatches synchronously, no outbox flush is needed.
/// </summary>
public static class FundingEventHelpers
{
    /// <summary>
    /// Publishes an OnboardCustomerFromLoanRequestedIntegrationEvent via the in-memory event bus.
    /// The Customer module's handler runs synchronously and creates the customer.
    /// </summary>
    public static async Task PublishOnboardCustomerFromLoanEventAsync(
        EventConsumerTestFixture fixture,
        string loanId = "LOAN-TEST-001",
        int? homeCenterNumber = null)
    {
        using var scope = fixture.Services.CreateScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        var integrationEvent = new OnboardCustomerFromLoanRequestedIntegrationEvent(
            Id: Guid.NewGuid(),
            OccurredOnUtc: DateTime.UtcNow,
            LoanId: loanId,
            HomeCenterNumber: homeCenterNumber ?? EventConsumerTestFixture.TestHomeCenterNumber);

        await eventBus.PublishAsync(integrationEvent);
    }
}
