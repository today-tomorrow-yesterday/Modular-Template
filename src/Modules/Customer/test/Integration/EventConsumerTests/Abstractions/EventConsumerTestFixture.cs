using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application.Customers.OnboardCustomerFromLoan;
using Modules.Customer.Integration.Shared;

namespace Modules.Customer.EventConsumerTests.Abstractions;

// Test fixture for verifying that integration events from other modules are
// correctly consumed by the Customer module and result in customer creation.
//
// Replaces the StubVmfLosAdapter with FakeVmfLosAdapter so that
// OnboardCustomerFromLoanCommand returns borrower data instead of null.
//
// Resets the customer_dev database between tests so each test starts clean.
public class EventConsumerTestFixture : CustomerTestFixtureBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddSingleton<IVmfLosAdapter, FakeVmfLosAdapter>();
    }
}
