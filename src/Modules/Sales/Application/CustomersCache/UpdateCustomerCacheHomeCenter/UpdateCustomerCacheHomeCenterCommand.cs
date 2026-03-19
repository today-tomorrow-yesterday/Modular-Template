using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheHomeCenter;

public sealed record UpdateCustomerCacheHomeCenterCommand(
    Guid RefPublicId,
    int NewHomeCenterNumber) : ICommand;
