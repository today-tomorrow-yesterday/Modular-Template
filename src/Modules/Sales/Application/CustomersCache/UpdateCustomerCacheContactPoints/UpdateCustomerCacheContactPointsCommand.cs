using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheContactPoints;

public sealed record UpdateCustomerCacheContactPointsCommand(
    Guid RefPublicId,
    string? Email,
    string? Phone) : ICommand;
