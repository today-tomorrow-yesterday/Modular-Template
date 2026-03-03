using Modules.Sales.Domain.PartiesCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheName;

public sealed record UpdatePartyCacheNameCommand(
    Guid RefPublicId,
    PartyType PartyType,
    string DisplayName,
    string? FirstName,
    string? MiddleName,
    string? LastName,
    string? OrganizationName) : ICommand;
