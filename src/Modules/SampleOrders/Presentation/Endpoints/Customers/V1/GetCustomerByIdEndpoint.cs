using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Customers.GetCustomer;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Customers.V1;

internal sealed class GetCustomerByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{customerId:int}", GetCustomerByIdAsync)
            .WithSummary("Get a customer by ID")
            .WithDescription("Retrieves a customer by its unique identifier.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<CustomerResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetCustomerByIdAsync(
        int customerId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerQuery(customerId);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(Results.Ok, ApiResults.Problem);
    }
}
