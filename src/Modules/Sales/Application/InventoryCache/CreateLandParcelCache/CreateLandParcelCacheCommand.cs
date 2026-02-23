using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.CreateLandParcelCache;

public sealed record CreateLandParcelCacheCommand(LandParcelCache Cache) : ICommand;
