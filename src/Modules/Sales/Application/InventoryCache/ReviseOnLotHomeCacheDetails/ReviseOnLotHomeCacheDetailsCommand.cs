using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.ReviseOnLotHomeCacheDetails;

public sealed record ReviseOnLotHomeCacheDetailsCommand(OnLotHomeCache Cache) : ICommand;
