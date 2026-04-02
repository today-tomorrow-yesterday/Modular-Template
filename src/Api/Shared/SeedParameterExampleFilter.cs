using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ModularTemplate.Api.Shared;

/// <summary>
/// Auto-populates Swagger route parameter examples with sample GUIDs.
/// These are placeholder UUIDv7 values for Swagger UI — actual seeded PublicIds
/// are generated at runtime via Guid.CreateVersion7() and will differ.
/// </summary>
internal sealed class SeedParameterExampleFilter : IOperationFilter
{
    private const string SampleOrderId = "01970f2e-0000-7000-8000-000000000001";
    private const string SampleCustomerId = "01970f2e-0000-7000-8000-000000000002";
    private const string SampleProductId = "01970f2e-0000-7000-8000-000000000003";
    private const string SampleCatalogId = "01970f2e-0000-7000-8000-000000000004";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
        {
            return;
        }

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
