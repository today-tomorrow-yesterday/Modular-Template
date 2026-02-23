using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Tax.UpdatePackageTax;
using Modules.Sales.Presentation.Endpoints.V1.Packages;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Tax;

internal sealed class UpdatePackageTaxEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/tax", HandleAsync)
            .WithSummary("Save tax configuration")
            .WithDescription("Saves PreviouslyTitled, TaxExemptionId, and QuestionAnswers. Clears prior calculation. Does NOT trigger iSeries calls.")
            .WithName("UpdatePackageTax")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PackageUpdatedResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageTaxRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageTaxCommand(
            publicPackageId,
            request.PreviouslyTitled,
            request.TaxExemptionId,
            request.QuestionAnswers
                .Select(q => new TaxQuestionAnswerRequest(q.QuestionNumber, q.Answer))
                .ToList());

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResults.Problem);
    }
}

public sealed record UpdatePackageTaxRequest(
    string? PreviouslyTitled,
    int? TaxExemptionId,
    UpdateTaxQuestionAnswer[] QuestionAnswers);

public sealed record UpdateTaxQuestionAnswer(
    int QuestionNumber,
    bool Answer);
