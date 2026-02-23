using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.ReviseOnLotHomeCachePrice;

public sealed record ReviseOnLotHomeCachePriceCommand(OnLotHomeCache Cache) : ICommand;
