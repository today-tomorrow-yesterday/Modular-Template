using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheContactPoints;

public sealed record UpdatePartyCacheContactPointsCommand(
    Guid RefPublicId,
    string? Email,
    string? Phone) : ICommand;
