using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ModularTemplate.Api.Shared;

/// <summary>
/// Auto-populates Swagger route parameter examples with sample IDs.
/// Fires for every endpoint — no per-endpoint configuration needed.
/// </summary>
internal sealed class SeedParameterExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null) return;

        foreach (var param in operation.Parameters.OfType<OpenApiParameter>())
        {
            param.Example = param.Name switch
            {
                "orderId" => JsonValue.Create(1),
                "customerId" => JsonValue.Create(1),
                "productId" => JsonValue.Create(1),
                "catalogId" => JsonValue.Create(1),
                _ => param.Example
            };
        }
    }
}
