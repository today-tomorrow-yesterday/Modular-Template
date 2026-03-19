using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Customer.Application.Customers.GetCustomerByPublicId;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Customer.Presentation.Endpoints.V1.Customers;

internal sealed class GetCustomerByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", GetCustomerByIdAsync)
            .WithSummary("Get a customer by public ID")
            .WithDescription("Retrieves a customer by their unique public identifier (GUID).")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<CustomerResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetCustomerByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerByPublicIdQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.Match(ApiResponse.Ok, ApiResponse.Problem);
    }
}
