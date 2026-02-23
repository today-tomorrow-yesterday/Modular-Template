using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(int CustomerId) : IQuery<CustomerResponse>;
