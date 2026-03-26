using Modules.Customer.Application.Customers.OnboardCustomerFromLoan;

namespace Modules.Customer.Integration.Shared;

/// <summary>
/// Fake implementation of IVmfLosAdapter for integration tests.
/// Returns a single borrower so that OnboardCustomerFromLoanCommand succeeds.
/// </summary>
public sealed class FakeVmfLosAdapter : IVmfLosAdapter
{
    public const string DefaultFirstName = "Loan";
    public const string DefaultLastName = "Borrower";
    public const string DefaultEmail = "borrower@test.com";
    public const string DefaultCellPhone = "555-0200";

    public Task<VmfLosResponse?> GetBorrowerByLoanIdAsync(
        string loanId,
        CancellationToken cancellationToken = default)
    {
        var response = new VmfLosResponse
        {
            Borrowers =
            [
                new VmfBorrower
                {
                    FirstName = DefaultFirstName,
                    LastName = DefaultLastName,
                    Email = DefaultEmail,
                    CellPhone = DefaultCellPhone,
                    DateOfBirth = new DateTime(1985, 6, 15)
                }
            ]
        };

        return Task.FromResult<VmfLosResponse?>(response);
    }
}
