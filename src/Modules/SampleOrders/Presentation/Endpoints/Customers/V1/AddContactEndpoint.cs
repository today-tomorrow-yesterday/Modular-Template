using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.SampleOrders.Application.Customers.AddContact;
using Modules.SampleOrders.Domain.Customers;
using ModularTemplate.Presentation.Endpoints;
using ModularTemplate.Presentation.Results;

namespace Modules.SampleOrders.Presentation.Endpoints.Customers.V1;

internal sealed class AddContactEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{customerId:int}/contacts", AddContactAsync)
            .WithMetadata(new RequestBodyExample("""{ "type": "Email", "value": "jane.doe@example.com", "isPrimary": true }"""))
            .WithName("AddCustomerContact")
            .WithSummary("Add a contact to a customer")
            .WithDescription("Adds a contact (email, phone, mobile) to an existing customer.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> AddContactAsync(
        int customerId,
        AddContactRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new AddContactCommand(
            customerId,
            request.Type,
            request.Value,
            request.IsPrimary);

        var result = await sender.Send(command, cancellationToken);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record AddContactRequest(
    ContactType Type,
    string Value,
    bool IsPrimary = false);
