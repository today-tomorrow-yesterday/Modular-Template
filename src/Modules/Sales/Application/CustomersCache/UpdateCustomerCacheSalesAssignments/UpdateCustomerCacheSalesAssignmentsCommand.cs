using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheSalesAssignments;

public sealed record UpdateCustomerCacheSalesAssignmentsCommand(
    Guid RefPublicId,
    string? PrimaryFederatedId,
    string? PrimaryFirstName,
    string? PrimaryLastName,
    string? SecondaryFederatedId,
    string? SecondaryFirstName,
    string? SecondaryLastName) : ICommand;
