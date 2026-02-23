using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Tax.GetTaxQuestions;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Tax;

internal sealed class GetTaxQuestionsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/tax/questionnaire", HandleAsync)
            .WithSummary("Get tax questionnaire")
            .WithDescription("Returns state-specific tax questions from CDC reference data.")
            .WithName("GetTaxQuestions")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<IReadOnlyCollection<TaxQuestionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        int homeTypeId,
        string state,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetTaxQuestionsQuery(state, homeTypeId);

        var result = await sender.Send(query, ct);

        return result.Match(
            questions => Results.Ok(questions.Select(q => new TaxQuestionResponse(
                q.QuestionNumber,
                q.Text)).ToList()),
            ApiResults.Problem);
    }
}

public sealed record TaxQuestionResponse(
    int QuestionNumber,
    string Text);
