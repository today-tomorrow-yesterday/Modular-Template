using Rtl.Core.Domain.Events;

namespace Modules.Customer.Domain.Customers.Events;

public sealed record CustomerHomeCenterChangedDomainEvent(int NewHomeCenterNumber) : DomainEvent;
