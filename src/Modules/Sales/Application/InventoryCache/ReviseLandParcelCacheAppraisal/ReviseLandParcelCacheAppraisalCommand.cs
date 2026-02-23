using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheAppraisal;

public sealed record ReviseLandParcelCacheAppraisalCommand(LandParcelCache Cache) : ICommand;
