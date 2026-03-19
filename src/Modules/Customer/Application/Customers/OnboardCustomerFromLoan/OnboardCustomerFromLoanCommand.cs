using Rtl.Core.Application.Messaging;

namespace Modules.Customer.Application.Customers.OnboardCustomerFromLoan;

public sealed record OnboardCustomerFromLoanCommand(
    string LoanId,
    int HomeCenterNumber) : ICommand<Guid>;
