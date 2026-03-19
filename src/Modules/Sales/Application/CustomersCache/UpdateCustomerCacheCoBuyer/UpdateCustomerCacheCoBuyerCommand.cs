using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.CustomersCache.UpdateCustomerCacheCoBuyer;

public sealed record UpdateCustomerCacheCoBuyerCommand(
    Guid RefPublicId,
    string? CoBuyerFirstName,
    string? CoBuyerLastName) : ICommand;
