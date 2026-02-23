using Modules.SampleOrders.Application.Customers.GetCustomer;
using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.GetCustomers;

public sealed record GetCustomersQuery(int? Limit = 100) : IQuery<IReadOnlyCollection<CustomerResponse>>;
