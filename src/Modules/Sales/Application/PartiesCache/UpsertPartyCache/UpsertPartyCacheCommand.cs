using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpsertPartyCache;

public sealed record UpsertPartyCacheCommand(
    PartyCache PartyCache,
    PartyPersonCache? PersonCache,
    PartyOrganizationCache? OrganizationCache) : ICommand;
