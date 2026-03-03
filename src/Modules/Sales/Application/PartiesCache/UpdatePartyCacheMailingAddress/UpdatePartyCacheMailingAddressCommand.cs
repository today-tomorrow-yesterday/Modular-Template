using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.PartiesCache.UpdatePartyCacheMailingAddress;

public sealed record UpdatePartyCacheMailingAddressCommand(
    Guid RefPublicId) : ICommand;
