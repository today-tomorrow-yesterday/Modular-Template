namespace Modules.SampleOrders.Application.Customers.GetCustomer;

public sealed record CustomerResponse(
    Guid PublicId,
    string FirstName,
    string? MiddleName,
    string LastName,
    string FullName,
    string? Email,
    DateTime CreatedAtUtc,
    Guid CreatedByUserId,
    DateTime? ModifiedAtUtc,
    Guid? ModifiedByUserId);
