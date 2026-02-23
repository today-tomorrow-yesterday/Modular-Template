namespace Modules.Customer.Application.Parties.OnboardPersonFromLoan;

// Adapter for VMF LOS (Loan Origination System) GraphQL API.
// Synchronous queries only — no CDC capability.
public interface IVmfLosAdapter
{
    Task<VmfLosResponse?> GetBorrowerByLoanIdAsync(string loanId, CancellationToken cancellationToken = default);
}

public sealed record VmfLosResponse
{
    public VmfBorrower[] Borrowers { get; init; } = [];
    public int? ProspectorId { get; init; } // Dedup key — existing CRM CustomerId (prospectorId from customDataPoints)
}

public sealed record VmfBorrower
{
    public string FirstName { get; init; } = null!;
    public string? MiddleName { get; init; }
    public string LastName { get; init; } = null!;
    public string? Suffix { get; init; }
    public string? Address1 { get; init; }
    public string? Address2 { get; init; }
    public string? City { get; init; }
    public string? StateId { get; init; }
    public string? Zip { get; init; }
    public string? CellPhone { get; init; }
    public string? HomePhone { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Email { get; init; }
}
