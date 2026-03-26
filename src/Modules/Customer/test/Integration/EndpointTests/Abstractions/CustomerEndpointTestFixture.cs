using Modules.Customer.Integration.Shared;

namespace Modules.Customer.EndpointTests.Abstractions;

// Test fixture for Customer API endpoint tests (GetCustomerById).
//
// Inherits database reset and configuration from CustomerTestFixtureBase.
// No additional cache seeding helpers needed — Customer entities are created
// via MediatR commands (SyncCustomerFromCrmCommand) during test arrangement.
public class CustomerEndpointTestFixture : CustomerTestFixtureBase;
