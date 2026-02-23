using Modules.Customer.Domain.Parties.Enums;
using Rtl.Core.Domain.Events;

namespace Modules.Customer.Domain.Parties.Events;

public sealed record PartyLifecycleAdvancedDomainEvent(LifecycleStage NewStage) : DomainEvent;
