using Modules.Customer.Domain.Customers.Enums;
using Rtl.Core.Domain.Events;

namespace Modules.Customer.Domain.Customers.Events;

public sealed record CustomerLifecycleAdvancedDomainEvent(LifecycleStage NewStage) : DomainEvent;
