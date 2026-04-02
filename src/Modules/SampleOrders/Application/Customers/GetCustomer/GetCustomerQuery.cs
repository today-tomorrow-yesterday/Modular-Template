using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(Guid PublicCustomerId) : IQuery<CustomerResponse>;
