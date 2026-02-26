using Microsoft.OpenApi;
using Rtl.Core.Presentation.Endpoints;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace Rtl.Core.Api.Shared;

/// <summary>
/// Reads <see cref="RequestBodyExample"/> metadata from endpoints and sets the OpenAPI request body example.
/// Register once in AddSwaggerGen — individual examples live on each endpoint.
/// </summary>
internal sealed class RequestBodyExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var example = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<RequestBodyExample>()
            .FirstOrDefault();

        if (example is null || operation.RequestBody?.Content is null)
            return;

        var node = JsonNode.Parse(example.Json);
        foreach (var mediaType in operation.RequestBody.Content.Values)
        {
            mediaType.Example = node;
        }
    }
}
