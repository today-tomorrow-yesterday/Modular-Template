using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ModularTemplate.Api.Shared;

/// <summary>
/// Auto-populates Swagger route parameter examples with sample GUIDs.
/// Fires for every endpoint — no per-endpoint configuration needed.
/// </summary>
internal sealed class SeedParameterExampleFilter : IOperationFilter
{
    private const string SampleOrderId = "01970000-0000-7000-8000-000000000001";
    private const string SampleCustomerId = "01970000-0000-7000-8000-000000000002";
    private const string SampleProductId = "01970000-0000-7000-8000-000000000003";
    private const string SampleCatalogId = "01970000-0000-7000-8000-000000000004";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null) return;

        foreach (var param in operation.Parameters.OfType<OpenApiParameter>())
        {
            param.Example = param.Name switch
            {
                "orderId" => JsonValue.Create(SampleOrderId),
                "customerId" => JsonValue.Create(SampleCustomerId),
                "productId" => JsonValue.Create(SampleProductId),
                "catalogId" => JsonValue.Create(SampleCatalogId),
                _ => param.Example
            };
        }
    }
}
