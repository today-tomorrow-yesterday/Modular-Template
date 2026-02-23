using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheDetails;

public sealed record ReviseLandParcelCacheDetailsCommand(LandParcelCache Cache) : ICommand;
