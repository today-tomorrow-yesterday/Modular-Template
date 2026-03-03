using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheLifecycle;

public sealed record UpdatePartyCacheLifecycleCommand(
    Guid RefPublicId,
    LifecycleStage NewLifecycleStage) : ICommand;
