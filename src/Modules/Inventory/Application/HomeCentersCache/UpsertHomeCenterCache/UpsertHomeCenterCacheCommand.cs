using Modules.Inventory.Domain.HomeCentersCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.HomeCentersCache.UpsertHomeCenterCache;

public sealed record UpsertHomeCenterCacheCommand(HomeCenterCache Cache) : ICommand;
