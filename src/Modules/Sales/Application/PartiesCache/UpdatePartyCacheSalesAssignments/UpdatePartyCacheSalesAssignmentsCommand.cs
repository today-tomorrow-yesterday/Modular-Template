using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheSalesAssignments;

public sealed record UpdatePartyCacheSalesAssignmentsCommand(
    int RefPartyId,
    string? PrimaryFederatedId,
    string? PrimaryFirstName,
    string? PrimaryLastName,
    string? SecondaryFederatedId,
    string? SecondaryFirstName,
    string? SecondaryLastName) : ICommand;
