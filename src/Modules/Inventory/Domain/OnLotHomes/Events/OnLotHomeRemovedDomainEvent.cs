using Rtl.Core.Domain.Events;

namespace Modules.Inventory.Domain.OnLotHomes.Events;

public sealed record OnLotHomeRemovedDomainEvent(Guid PublicId) : DomainEvent;
