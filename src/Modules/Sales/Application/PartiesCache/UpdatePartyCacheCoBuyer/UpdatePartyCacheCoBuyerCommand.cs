using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheCoBuyer;

public sealed record UpdatePartyCacheCoBuyerCommand(
    int RefPartyId,
    string? CoBuyerFirstName,
    string? CoBuyerLastName) : ICommand;
