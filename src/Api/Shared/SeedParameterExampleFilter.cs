using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace Rtl.Core.Api.Shared;

/// <summary>
/// Auto-populates Swagger route parameter examples with deterministic seed GUIDs.
/// Fires for every endpoint — no per-endpoint configuration needed.
/// GUIDs match <c>SeedConstants.DeterministicGuid()</c> output for index 0.
/// </summary>
internal sealed class SeedParameterExampleFilter : IOperationFilter
{
    // Deterministic GUIDs from SeedConstants.DeterministicGuid() — SHA-256 hashes of fixed strings.
    // These NEVER change between runs, developers, or database recreations.
    private const string DefaultSaleId = "c5331d22-bb8e-2c4f-bc06-589c0aad842c";
    private const string DefaultPackageId = "ab743b1f-bba5-574d-83f8-3c0dd1424ab3";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null) return;

        foreach (var param in operation.Parameters.OfType<OpenApiParameter>())
        {
            param.Example = param.Name switch
            {
                "publicSaleId" => JsonValue.Create(DefaultSaleId),
                "publicPackageId" => JsonValue.Create(DefaultPackageId),
                _ => param.Example
            };
        }
    }
}
