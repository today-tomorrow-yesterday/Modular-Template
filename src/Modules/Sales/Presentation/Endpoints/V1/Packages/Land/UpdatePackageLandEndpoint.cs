using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageLand;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.Land;

internal sealed class UpdatePackageLandEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/land", HandleAsync)
            .WithSummary("Update package land section")
            .WithDescription("Upserts the land section. Conditional fields based on land type path.")
            .WithName("UpdatePackageLand")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<PackageUpdatedResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageLandRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageLandCommand(
            PackagePublicId: publicPackageId,
            SalePrice: request.SalePrice,
            EstimatedCost: request.EstimatedCost,
            RetailSalePrice: request.RetailSalePrice,
            LandPurchaseType: request.LandPurchaseType,
            TypeOfLandWanted: request.TypeOfLandWanted,
            CustomerLandType: request.CustomerLandType,
            LandInclusion: request.LandInclusion,
            LandStockNumber: request.LandStockNumber,
            LandSalesPrice: request.LandSalesPrice,
            LandCost: request.LandCost,
            PropertyOwner: request.PropertyOwner,
            FinancedBy: request.FinancedBy,
            EstimatedValue: request.EstimatedValue,
            SizeInAcres: request.SizeInAcres,
            PayoffAmountFinancing: request.PayoffAmountFinancing,
            LandEquity: request.LandEquity,
            OriginalPurchaseDate: request.OriginalPurchaseDate,
            OriginalPurchasePrice: request.OriginalPurchasePrice,
            Realtor: request.Realtor,
            PurchasePrice: request.PurchasePrice,
            PropertyOwnerPhoneNumber: request.PropertyOwnerPhoneNumber,
            PropertyLotRent: request.PropertyLotRent,
            CommunityNumber: request.CommunityNumber,
            CommunityName: request.CommunityName,
            CommunityManagerName: request.CommunityManagerName,
            CommunityManagerPhoneNumber: request.CommunityManagerPhoneNumber,
            CommunityManagerEmail: request.CommunityManagerEmail,
            CommunityMonthlyCost: request.CommunityMonthlyCost);

        var result = await sender.Send(command, ct);

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
            "salePrice": 45000.00,
            "estimatedCost": 40000.00,
            "retailSalePrice": 50000.00,
            "landPurchaseType": "CustomerHasLand",
            "typeOfLandWanted": null,
            "customerLandType": "CustomerOwnedLand",
            "landInclusion": "CustomerLandPayoff",
            "landStockNumber": null,
            "landSalesPrice": null,
            "landCost": null,
            "propertyOwner": "John Doe",
            "financedBy": "Local Bank",
            "estimatedValue": 55000.00,
            "sizeInAcres": 2.5,
            "payoffAmountFinancing": 20000.00,
            "landEquity": 35000.00,
            "originalPurchaseDate": "2018-06-15T00:00:00Z",
            "originalPurchasePrice": 30000.00,
            "realtor": null,
            "purchasePrice": null,
            "propertyOwnerPhoneNumber": "8651234567",
            "propertyLotRent": null,
            "communityNumber": null,
            "communityName": null,
            "communityManagerName": null,
            "communityManagerPhoneNumber": null,
            "communityManagerEmail": null,
            "communityMonthlyCost": null
        }
        """;
    }
}

public sealed record UpdatePackageLandRequest(
    decimal SalePrice,
    decimal EstimatedCost,
    decimal RetailSalePrice,
    string LandPurchaseType,
    string? TypeOfLandWanted,
    string? CustomerLandType,
    string? LandInclusion,
    string? LandStockNumber,
    decimal? LandSalesPrice,
    decimal? LandCost,
    string? PropertyOwner,
    string? FinancedBy,
    decimal? EstimatedValue,
    decimal? SizeInAcres,
    decimal? PayoffAmountFinancing,
    decimal? LandEquity,
    DateTime? OriginalPurchaseDate,
    decimal? OriginalPurchasePrice,
    string? Realtor,
    decimal? PurchasePrice,
    string? PropertyOwnerPhoneNumber,
    decimal? PropertyLotRent,
    int? CommunityNumber,
    string? CommunityName,
    string? CommunityManagerName,
    string? CommunityManagerPhoneNumber,
    string? CommunityManagerEmail,
    decimal? CommunityMonthlyCost);
