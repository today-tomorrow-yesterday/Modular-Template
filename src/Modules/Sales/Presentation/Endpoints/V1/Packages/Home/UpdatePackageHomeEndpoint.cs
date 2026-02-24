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
            .Produces<PackageUpdatedResponse>(StatusCodes.Status200OK)
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
            r => Results.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResults.Problem);
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "salePrice": 85000.00,
            "estimatedCost": 60000.00,
            "retailSalePrice": 90000.00,
            "stockNumber": "STK-001",
            "homeType": 0,
            "homeSourceType": 0,
            "modularType": 0,
            "vendor": "Clayton",
            "make": "Clayton",
            "model": "Model-A",
            "modelYear": 2025,
            "lengthInFeet": 76.0,
            "widthInFeet": 28.0,
            "bedrooms": 3,
            "bathrooms": 2.0,
            "squareFootage": "2128",
            "serialNumbers": ["SER-001", "SER-002"],
            "baseCost": 55000.00,
            "optionsCost": 3000.00,
            "freightCost": 2000.00,
            "invoiceCost": 60000.00,
            "netInvoice": 59000.00,
            "grossCost": 61000.00,
            "taxIncludedOnInvoice": 500.00,
            "numberOfWheels": 8,
            "numberOfAxles": 4,
            "wheelAndAxlesOption": 0,
            "numberOfFloorSections": 2,
            "carrierFrameDeposit": 1500.00,
            "rebateOnMfgInvoice": 200.00,
            "claytonBuilt": true,
            "buildType": "Standard",
            "inventoryReferenceId": "INV-12345",
            "stateAssociationAndMhiDues": 150.00,
            "partnerAssistance": 500.00,
            "distanceMiles": 125.5,
            "propertyType": "Residential",
            "purchaseOption": "Cash",
            "listingPrice": 92000.00,
            "accountNumber": "ACCT-001",
            "displayAccountId": "DISP-001",
            "streetAddress": "123 Main St",
            "city": "Maryville",
            "state": "TN",
            "zipCode": "37804"
        }
        """;
    }
}
