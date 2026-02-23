using Rtl.Core.Application.Messaging;

namespace Modules.Customer.Application.Parties.OnboardPersonFromLoan;

// VMF LOS adapter fetches borrower data from LoanId. Always creates a Person.
public sealed record OnboardPersonFromLoanCommand(
    string LoanId,
    int HomeCenterNumber) : ICommand<Guid>;
