using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheHomeCenter;

public sealed record UpdatePartyCacheHomeCenterCommand(
    Guid RefPublicId,
    int NewHomeCenterNumber) : ICommand;
