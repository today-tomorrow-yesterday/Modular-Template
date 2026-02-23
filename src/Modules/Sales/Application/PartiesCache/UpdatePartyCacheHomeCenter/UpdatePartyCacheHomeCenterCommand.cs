using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheHomeCenter;

public sealed record UpdatePartyCacheHomeCenterCommand(
    int RefPartyId,
    int NewHomeCenterNumber) : ICommand;
