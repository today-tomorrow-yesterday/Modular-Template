using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Customers.CreateCustomer;
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Customers.V1;

internal sealed class CreateCustomerEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/", CreateCustomerAsync)
            .WithMetadata(new RequestBodyExample("""{ "firstName": "Jane", "middleName": "M", "lastName": "Doe", "email": "jane.doe@example.com", "dateOfBirth": "1990-05-15" }"""))
            .WithName("CreateCustomer")
            .WithSummary("Create a new customer")
            .WithDescription("Creates a new customer with the specified name and email.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<CreateCustomerResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> CreateCustomerAsync(
        CreateCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.Email,
            request.DateOfBirth);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            publicId => ApiResponse.Created($"/customers/{publicId}", new CreateCustomerResponse(publicId)),
            ApiResponse.Problem);
    }
}

public sealed record CreateCustomerRequest(
    string FirstName,
    string? MiddleName,
    string LastName,
    string? Email,
    DateOnly? DateOfBirth = null);

public sealed record CreateCustomerResponse(Guid PublicId);
