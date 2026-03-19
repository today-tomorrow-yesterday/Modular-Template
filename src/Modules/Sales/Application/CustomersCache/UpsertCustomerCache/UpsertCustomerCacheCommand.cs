using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.CustomersCache.UpsertCustomerCache;

public sealed record UpsertCustomerCacheCommand(CustomerCache CustomerCache) : ICommand;
