using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheMailingAddress;

public sealed record UpdateCustomerCacheMailingAddressCommand(
    Guid RefPublicId) : ICommand;
