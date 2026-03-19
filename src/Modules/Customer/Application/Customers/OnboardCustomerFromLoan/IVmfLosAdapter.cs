namespace Modules.Customer.Application.Customers.OnboardCustomerFromLoan;

public interface IVmfLosAdapter
{
    Task<VmfLosResponse?> GetBorrowerByLoanIdAsync(string loanId, CancellationToken cancellationToken = default);
}

public sealed record VmfLosResponse
{
    public VmfBorrower[] Borrowers { get; init; } = [];
    public int? ProspectorId { get; init; }
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
