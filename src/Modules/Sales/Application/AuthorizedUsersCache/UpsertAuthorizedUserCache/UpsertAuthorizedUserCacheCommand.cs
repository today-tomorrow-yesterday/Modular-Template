using Modules.Sales.Domain.AuthorizedUsersCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.AuthorizedUsersCache.UpsertAuthorizedUserCache;

public sealed record UpsertAuthorizedUserCacheCommand(AuthorizedUserCache Cache) : ICommand;
