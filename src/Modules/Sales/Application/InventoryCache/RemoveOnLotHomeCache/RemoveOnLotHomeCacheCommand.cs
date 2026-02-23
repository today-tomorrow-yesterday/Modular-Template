using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.RemoveOnLotHomeCache;

public sealed record RemoveOnLotHomeCacheCommand(
    int RefOnLotHomeId,
    int HomeCenterNumber,
    string StockNumber) : ICommand;
