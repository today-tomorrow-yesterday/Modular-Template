using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.RemoveLandParcelCache;

public sealed record RemoveLandParcelCacheCommand(
    Guid PublicLandParcelId) : ICommand;
