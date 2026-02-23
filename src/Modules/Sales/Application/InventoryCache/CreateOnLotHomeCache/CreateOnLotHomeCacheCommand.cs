using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.CreateOnLotHomeCache;

public sealed record CreateOnLotHomeCacheCommand(OnLotHomeCache Cache) : ICommand;
