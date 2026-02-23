using Rtl.Core.Domain.Events;

namespace Modules.Customer.Domain.Parties.Events;

public sealed record PartyHomeCenterChangedDomainEvent(int NewHomeCenterNumber) : DomainEvent;
