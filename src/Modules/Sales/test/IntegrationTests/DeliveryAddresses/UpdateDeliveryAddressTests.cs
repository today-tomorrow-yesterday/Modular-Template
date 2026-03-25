using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Application.Packages.GetPackageById;
using Modules.Sales.Domain.Packages.Insurance;
using Modules.Sales.IntegrationTests.Abstractions;
using Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Insurance;
using Modules.Sales.Presentation.Endpoints.V1.Packages.Warranty;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.IntegrationTests.DeliveryAddresses;

// PUT /api/v1/sales/{saleId}/delivery-address
//
// Tests the UpdateDeliveryAddress endpoint.
//
// Tests:
// - Valid update -> 200 OK, verify changes persisted via GET
// - State change -> flags tax recalculation on the package
// - Occupancy becomes ineligible ("Rental") -> removes insurance and warranty lines
// - No existing address -> 404 Not Found
public class UpdateDeliveryAddressTests(SalesTestFactory factory) : SalesIntegrationTestBase(factory)
{
    private string Endpoint => $"/api/v1/sales/{SaleId}/delivery-address";

    [Fact]
    public async Task Should_ReturnOk_AndPersistChanges()
    {
        // Arrange
        await ArrangeSaleWithDeliveryAsync();

        var request = new UpdateDeliveryAddressRequest(
            OccupancyType: "Secondary Residence",
            IsWithinCityLimits: false,
            AddressLine1: "999 Lake Rd",
            City: "Knoxville",
            County: "Knox",
            State: "TN",
            PostalCode: "37902");

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have returned 200 OK

        var getResponse = await Client.GetAsync<ApiEnvelope<DeliveryAddressResponse>>(Endpoint);
        Assert.NotNull(getResponse?.Data);                                         // Should have returned updated address
        Assert.Equal("Secondary Residence", getResponse.Data.OccupancyType);                  // Should have updated occupancy type
        Assert.False(getResponse.Data.IsWithinCityLimits);                         // Should have updated city limits flag
        Assert.Equal("999 Lake Rd", getResponse.Data.AddressLine1);                // Should have updated address line 1
        Assert.Equal("Knoxville", getResponse.Data.City);                          // Should have updated city
        Assert.Equal("Knox", getResponse.Data.County);                             // Should have updated county
        Assert.Equal("37902", getResponse.Data.PostalCode);                        // Should have updated postal code
    }

    [Fact]
    public async Task Should_FlagTaxRecalculation_WhenStateChanges()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();

        var request = new UpdateDeliveryAddressRequest(
            OccupancyType: "Primary Residence",
            IsWithinCityLimits: true,
            AddressLine1: "123 Test St",
            City: "Atlanta",
            County: "Fulton",
            State: "GA",
            PostalCode: "30301");

        // Act
        await Client.PutAndAssertOkAsync(Endpoint, request);

        // Assert
        var updatedPackage = await GetPackageAsync();
        Assert.True(updatedPackage.MustRecalculateTaxes);                          // Should have flagged tax recalculation after state change
    }

    [Fact]
    public async Task Should_RemoveInsuranceAndWarranty_WhenOccupancyBecomesIneligible()
    {
        // Arrange
        await ArrangeSaleWithHomeAsync();

        // Add insurance
        var insuranceRequest = new UpdatePackageInsuranceRequest(
            InsuranceType: nameof(InsuranceType.HomeFirst),
            CoverageAmount: 300_000m,
            HasFoundationOrMasonry: false,
            InParkOrSubdivision: false,
            IsLandOwnedByCustomer: true,
            IsPremiumFinanced: true,
            CompanyName: "HomeFirst Insurance Co",
            MaxCoverage: 350_000m,
            TotalPremium: 1_500m);
        await Client.PutAndAssertOkAsync($"/api/v1/packages/{PackageId}/insurance", insuranceRequest);

        // Add warranty
        var warrantyRequest = new UpdatePackageWarrantyRequest(
            WarrantySelected: true,
            WarrantyAmount: 1_500m);
        await Client.PutAndAssertOkAsync($"/api/v1/packages/{PackageId}/warranty", warrantyRequest);

        // Confirm both exist
        var packageBeforeUpdate = await GetPackageAsync();
        Assert.NotNull(packageBeforeUpdate.Insurance);                             // Should have insurance before occupancy change
        Assert.NotNull(packageBeforeUpdate.Warranty);                              // Should have warranty before occupancy change

        // Act — change occupancy to "Rental" (ineligible for insurance and warranty)
        var request = new UpdateDeliveryAddressRequest(
            OccupancyType: "Rental",
            IsWithinCityLimits: true,
            AddressLine1: "123 Test St",
            City: "Nashville",
            County: "Davidson",
            State: "TN",
            PostalCode: "37201");
        var response = await Client.PutAsJsonAsync(Endpoint, request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);                      // Should have accepted the occupancy update

        // Assert
        var updatedPackage = await GetPackageAsync();
        Assert.Null(updatedPackage.Insurance);                                     // Should have removed insurance for ineligible occupancy
        Assert.Null(updatedPackage.Warranty);                                      // Should have removed warranty for ineligible occupancy
    }

    [Fact]
    public async Task Should_ReturnNotFound_WhenNoAddressExists()
    {
        // Arrange
        await ArrangeSaleAsync();

        var request = new UpdateDeliveryAddressRequest(
            "Primary Residence", true, "123 Main St", "Maryville", "Blount", "TN", "37801");

        // Act
        var response = await Client.PutAsJsonAsync(Endpoint, request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);                // Should have returned 404 when no address exists
    }
}
