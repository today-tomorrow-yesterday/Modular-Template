using Rtl.Core.Application.EventBus;

namespace Modules.Funding.IntegrationEvents;

/// <summary>
/// Process trigger event published by Funding when a CDX/VMF Router request
/// arrives with a LoanId but no CustomerId.
/// Customers module subscribes and handles customer onboarding from VMF LOS.
/// </summary>
public sealed record OnboardCustomerFromLoanRequestedIntegrationEvent(
    Guid Id,
    DateTime OccurredOnUtc,
    string LoanId,
    int HomeCenterNumber) : IntegrationEvent(Id, OccurredOnUtc);
