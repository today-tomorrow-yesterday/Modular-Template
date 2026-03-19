using Modules.Sales.Domain.CustomersCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheLifecycle;

public sealed record UpdateCustomerCacheLifecycleCommand(
    Guid RefPublicId,
    LifecycleStage NewLifecycleStage) : ICommand;
