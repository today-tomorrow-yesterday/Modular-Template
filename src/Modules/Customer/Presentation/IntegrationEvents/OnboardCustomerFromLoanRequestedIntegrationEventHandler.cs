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
    : IIntegrationEventHandler<OnboardCustomerFromLoanRequestedIntegrationEvent>
{
    public async Task HandleAsync(
        OnboardCustomerFromLoanRequestedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing OnboardCustomerFromLoanRequested: LoanId={LoanId}, HomeCenterNumber={HomeCenterNumber}",
            integrationEvent.LoanId,
            integrationEvent.HomeCenterNumber);

        await sender.Send(
            new OnboardPersonFromLoanCommand(
                integrationEvent.LoanId,
                integrationEvent.HomeCenterNumber), cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((OnboardCustomerFromLoanRequestedIntegrationEvent)integrationEvent, cancellationToken);
}
