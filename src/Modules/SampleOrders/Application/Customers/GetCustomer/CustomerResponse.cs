namespace Modules.SampleOrders.Application.Customers.GetCustomer;

public sealed record CustomerResponse(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAtUtc,
    Guid CreatedByUserId,
    DateTime? ModifiedAtUtc,
    Guid? ModifiedByUserId);
