using System.Net;
using System.Net.Http.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Application.Customers.GetCustomerByPublicId;
using Modules.Customer.Application.Customers.SyncCustomerFromCrm;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.EndpointTests.Abstractions;
using Modules.Customer.Infrastructure.Persistence;
using Rtl.Core.Presentation.Results;

namespace Modules.Customer.EndpointTests.Customers;

// GET /api/v1/customers/{id}
//
// Tests:
// - Valid customer PublicId -> 200 OK with customer details
// - Unknown PublicId -> 404 Not Found
public class GetCustomerByIdTests(CustomerEndpointTestFixture fixture) : CustomerEndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_ReturnCustomer_WhenCustomerExists()
    {
        // Arrange — create a customer via SyncCustomerFromCrmCommand
        using var scope = Fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var command = new SyncCustomerFromCrmCommand(
            CrmCustomerId: 99001,
            HomeCenterNumber: TestHomeCenterNumber,
            LifecycleStage: LifecycleStage.Customer,
            FirstName: "Jane",
            MiddleName: null,
            LastName: "Doe",
            NameExtension: null,
            DateOfBirth: new DateOnly(1990, 1, 15),
            SalesAssignments: [],
            ContactPoints:
            [
                new SyncContactPointDto(ContactPointType.Email, "jane@test.com", true),
                new SyncContactPointDto(ContactPointType.Phone, "555-0100", false)
            ],
            Identifiers:
            [
                new SyncIdentifierDto(IdentifierType.CrmCustomerId, "99001")
            ],
            MailingAddress: null,
            SalesforceUrl: null,
            CreatedOn: DateTimeOffset.UtcNow,
            LastModifiedOn: DateTimeOffset.UtcNow);

        var result = await sender.Send(command);
        Assert.True(result.IsSuccess, $"SyncCustomerFromCrmCommand failed: {result.Error}");

        // Look up the customer's PublicId from the database
        var db = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
        var customer = await db.Set<Domain.Customers.Entities.Customer>()
            .FirstAsync(c => c.HomeCenterNumber == TestHomeCenterNumber);
        var publicId = customer.PublicId;

        // Act
        var response = await Client.GetAsync($"/api/v1/customers/{publicId}");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<CustomerResponse>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);       // Should have returned 200 OK
        Assert.NotNull(body?.Data);                                  // Should have returned customer data
        Assert.Equal(publicId, body.Data.PublicId);                  // Should match the created customer's PublicId
        Assert.Equal("Jane", body.Data.FirstName);                   // Should match first name
        Assert.Equal("Doe", body.Data.LastName);                     // Should match last name
        Assert.Equal(TestHomeCenterNumber, body.Data.HomeCenterNumber); // Should match home center
        Assert.Equal("Customer", body.Data.LifecycleStage);          // Should match lifecycle stage
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var unknownCustomerId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/customers/{unknownCustomerId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);  // Should have returned 404 Not Found
    }
}
