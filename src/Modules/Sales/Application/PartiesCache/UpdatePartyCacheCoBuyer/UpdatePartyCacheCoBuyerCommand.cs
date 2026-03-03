using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheCoBuyer;

public sealed record UpdatePartyCacheCoBuyerCommand(
    Guid RefPublicId,
    string? CoBuyerFirstName,
    string? CoBuyerLastName) : ICommand;
