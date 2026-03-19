using Modules.Customer.Application.Customers.OnboardCustomerFromLoan;

namespace Modules.Customer.Infrastructure.Adapters;

/// <summary>
/// Stub implementation of IVmfLosAdapter for development/testing.
/// Replace with real GraphQL adapter using CMH.VMF.LOS.SDK.Client in production.
/// </summary>
internal sealed class StubVmfLosAdapter : IVmfLosAdapter
{
    public Task<VmfLosResponse?> GetBorrowerByLoanIdAsync(
        string loanId,
        CancellationToken cancellationToken = default)
    {
        // Return null to simulate "borrower not found" by default.
        // In real implementation, this calls VMF LOS GraphQL API.
        return Task.FromResult<VmfLosResponse?>(null);
    }
}
