using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheName;

public sealed record UpdateCustomerCacheNameCommand(
    Guid RefPublicId,
    string DisplayName,
    string? FirstName,
    string? MiddleName,
    string? LastName) : ICommand;
