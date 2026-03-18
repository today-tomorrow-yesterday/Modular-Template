using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Customer.Application.Parties.OnboardPersonFromLoan;
using Modules.Funding.IntegrationEvents;
using Rtl.Core.Application.EventBus;

namespace Modules.Customer.Presentation.IntegrationEvents;

// Flow: Funding.OnboardCustomerFromLoanRequestedIntegrationEvent → Customer.OnboardPersonFromLoanCommand
internal sealed class OnboardCustomerFromLoanRequestedIntegrationEventHandler(
    ISender sender,
    ILogger<OnboardCustomerFromLoanRequestedIntegrationEventHandler> logger)
    : IntegrationEventHandler<OnboardCustomerFromLoanRequestedIntegrationEvent>
{
    public override async Task HandleAsync(
        OnboardCustomerFromLoanRequestedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing OnboardCustomerFromLoanRequested: LoanId={LoanId}, HomeCenterNumber={HomeCenterNumber}",
            integrationEvent.LoanId,
            integrationEvent.HomeCenterNumber);

        var result = await sender.Send(
            new OnboardPersonFromLoanCommand(
                integrationEvent.LoanId,
                integrationEvent.HomeCenterNumber), cancellationToken);

        if (result.IsFailure)
        {
            logger.LogError(
                "Failed to onboard customer from loan {LoanId}: {Error}",
                integrationEvent.LoanId,
                result.Error);
            throw new InvalidOperationException(
                $"Onboarding failed for LoanId={integrationEvent.LoanId}: {result.Error.Description}");
        }
    }
}
