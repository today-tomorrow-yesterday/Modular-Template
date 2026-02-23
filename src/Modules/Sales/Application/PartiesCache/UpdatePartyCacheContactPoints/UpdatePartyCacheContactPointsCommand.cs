using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheContactPoints;

public sealed record UpdatePartyCacheContactPointsCommand(
    int RefPartyId,
    string? Email,
    string? Phone) : ICommand;
