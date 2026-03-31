---
name: scaffold-endpoint
description: Use when adding a new Minimal API endpoint - creates endpoint class, request/response DTOs, command/query, handler, and Swagger metadata
---

# Scaffold Endpoint

Creates a complete Minimal API endpoint following the project's conventions.

## Endpoint Structure

Each endpoint is a static class in `src/Modules/{Module}/Presentation/Endpoints/V1/{Feature}/`.

### Query Endpoint (GET)

**File:** `src/Modules/{Module}/Presentation/Endpoints/V1/{Feature}/{EndpointName}Endpoint.cs`

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.{Module}.Application.{Feature}.{QueryName};
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.{Module}.Presentation.Endpoints.V1.{Feature};

internal sealed class {EndpointName}Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/{route}", HandleAsync)
            .WithName("{UniqueOperationId}")    // REQUIRED for Swagger
            .WithTags("{Feature}")
            .Produces<ApiEnvelope<{Response}>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [AsParameters] {Request} request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new {QueryName}(request.{Param}),
            cancellationToken);

        return result.Match(
            success: data => Results.Ok(ApiEnvelope.Success(data)),
            failure: error => error.ToProblem());
    }
}
```

### Command Endpoint (POST/PUT/DELETE)

```csharp
internal sealed class {EndpointName}Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/{route}", HandleAsync)
            .WithName("{UniqueOperationId}")    // REQUIRED for Swagger
            .WithTags("{Feature}")
            .Produces<ApiEnvelope<{Response}>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        {RequestBody} request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new {CommandName}(request.{Prop1}, request.{Prop2}),
            cancellationToken);

        return result.Match(
            success: data => Results.Created($"/api/v1/{route}/{data}", ApiEnvelope.Success(data)),
            failure: error => error.ToProblem());
    }
}
```

## Response DTOs

**File:** `src/Modules/{Module}/Application/{Feature}/{QueryName}/{QueryName}.cs`

```csharp
public sealed record {QueryName}(/* params */) : IQuery<{Response}>;

public sealed record {Response}(
    Guid PublicId,              // ALWAYS Guid — NEVER int Id
    // ... fields without Ref prefix
);
```

**Rules:**
- Use `Guid PublicId` for entity identity — never `int Id`
- Drop "Ref" prefix from response properties
- Nested DTOs for complex sub-objects (e.g., `AddressResponse`, `ContactPointResponse`)

## Request DTOs

```csharp
// For GET — use [AsParameters] with a record
public sealed record {Request}(Guid PublicId);           // route param
public sealed record {Request}(int HomeCenterNumber);    // query param

// For POST/PUT — request body record
public sealed record {RequestBody}(
    Guid PublicCustomerId,     // reference to another entity's PublicId
    int HomeCenterNumber,
    // ... input fields
);
```

## Swagger Rules

**Every endpoint MUST have `.WithName("UniqueOperationId")`.**

Without it, endpoints sharing `HandleAsync` as their handler method get the same `operationId` and Swagger UI groups them together incorrectly.

The `CustomOperationIds` in `Api/Shared/OpenApiExtensions.cs` checks `EndpointNameMetadata` first, falls back to method name. `.WithName()` sets `EndpointNameMetadata`.

**Naming convention for operation IDs:**
- `Get{Entity}` — single entity by ID
- `Get{Entity}List` or `Get{Entities}` — collection
- `Create{Entity}` — POST
- `Update{Entity}` — PUT
- `Delete{Entity}` — DELETE

## Checklist

- [ ] `.WithName("UniqueOperationId")` present — Swagger will break without it
- [ ] Response uses `Guid PublicId` — never `int Id`
- [ ] No "Ref" prefix in response properties
- [ ] `.WithTags("{Feature}")` for Swagger grouping
- [ ] `.Produces<>()` and `.ProducesProblem()` for OpenAPI schema
- [ ] Uses `ApiEnvelope.Success()` wrapper for consistent response shape
- [ ] Error results use `.ToProblem()` for ProblemDetails format
