using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.RemoveOnLotHomeCache;

public sealed record RemoveOnLotHomeCacheCommand(
    Guid PublicOnLotHomeId) : ICommand;
