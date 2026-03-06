using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageHome;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.Home;

internal sealed class UpdatePackageHomeEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/home", UpdatePackageHomeAsync)
            .WithSummary("Update package home section")
            .WithDescription("Adds or updates the home line on a package. PUT semantics — always replaces the existing home line.")
            .WithName("UpdatePackageHome")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<PackageUpdatedResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    private static async Task<IResult> UpdatePackageHomeAsync(
        Guid publicPackageId,
        UpdatePackageHomeRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePackageHomeCommand(publicPackageId, request);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            r => ApiResponse.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResponse.Problem);
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "salePrice": 340000.00,
            "estimatedCost": 250000.00,
            "retailSalePrice": 350000.00,
            "stockNumber": "STK0001",
            "homeType": 0,
            "homeSourceType": 0,
            "modularType": 0,
            "vendor": "CMH Manufacturing",
            "make": "Clayton",
            "model": "Summit",
            "modelYear": 2025,
            "lengthInFeet": 76.0,
            "widthInFeet": 28.0,
            "bedrooms": 3,
            "bathrooms": 2.0,
            "squareFootage": "2128",
            "serialNumbers": ["CLT834205AB"],
            "baseCost": 200000.00,
            "optionsCost": 30000.00,
            "freightCost": 20000.00,
            "invoiceCost": 250000.00,
            "netInvoice": 245000.00,
            "grossCost": 250000.00,
            "taxIncludedOnInvoice": 5000.00,
            "numberOfWheels": 16,
            "numberOfAxles": 4,
            "wheelAndAxlesOption": 0,
            "numberOfFloorSections": 2,
            "carrierFrameDeposit": 500.00,
            "rebateOnMfgInvoice": 5000.00,
            "claytonBuilt": true,
            "buildType": "Double",
            "inventoryReferenceId": null,
            "stateAssociationAndMhiDues": 150.00,
            "partnerAssistance": 500.00,
            "distanceMiles": 125.5,
            "propertyType": "DoubleWide",
            "purchaseOption": "Finance",
            "listingPrice": 355000.00,
            "accountNumber": "ACC-100234",
            "displayAccountId": "DA-1001",
            "streetAddress": "5000 Clayton Rd",
            "city": "Maryville",
            "state": "TN",
            "zipCode": "37801"
        }
        """;
    }
}
