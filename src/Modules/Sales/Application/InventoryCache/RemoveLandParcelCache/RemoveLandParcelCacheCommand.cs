using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.RemoveLandParcelCache;

public sealed record RemoveLandParcelCacheCommand(
    int RefLandParcelId,
    int HomeCenterNumber,
    string StockNumber) : ICommand;
