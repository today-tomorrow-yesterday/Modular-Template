# API Response Envelope Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Wrap every API response in a uniform `{ isSuccess, data, problemDetails }` envelope.

**Architecture:** Two custom `IResult` implementations (`SuccessEnvelopeResult<T>` and `ProblemEnvelopeResult`) resolve `HttpContext` at execution time for `requestId`/`traceId`/`instance`. A static `ApiResponse` helper class replaces `ApiResults` with the same method-group ergonomics. `GlobalExceptionHandler` and `FeatureFlagEndpointFilter` are updated to emit the same envelope.

**Tech Stack:** .NET 10, ASP.NET Minimal APIs, System.Text.Json, xUnit

**Design doc:** `docs/plans/2026-02-26-api-response-envelope-design.md`

---

## Task 1: Create Envelope Types

**Files:**
- Create: `src/Common/Presentation/Results/ApiEnvelope.cs`

**Step 1: Create the three envelope types in a single file**

```csharp
namespace Rtl.Core.Presentation.Results;

/// <summary>
/// Uniform API response envelope. Every endpoint returns this shape.
/// </summary>
public sealed class ApiEnvelope<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public ApiProblemEnvelope? ProblemDetails { get; init; }
}

/// <summary>
/// RFC 9457 Problem Details with errors list, requestId, and traceId.
/// </summary>
public sealed class ApiProblemEnvelope
{
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required int Status { get; init; }
    public required string Instance { get; init; }
    public required string RequestId { get; init; }
    public required string TraceId { get; init; }
    public required IReadOnlyList<ApiErrorDetail> Errors { get; init; }
}

/// <summary>
/// Individual error entry inside ProblemDetails.
/// </summary>
public sealed class ApiErrorDetail
{
    public required string Code { get; init; }
    public required string Description { get; init; }
}
```

> **Why classes not records:** System.Text.Json serializes `class` with `{ get; init; }` cleanly. Records work too, but classes avoid positional constructor ordering issues with deserialization in tests.

**Step 2: Build**

Run: `dotnet build src/Common/Presentation/Rtl.Core.Presentation.csproj`
Expected: Success, 0 errors

**Step 3: Commit**

```
feat: add API envelope types (ApiEnvelope<T>, ApiProblemEnvelope, ApiErrorDetail)
```

---

## Task 2: Create IResult Implementations and ApiResponse Helper

**Files:**
- Create: `src/Common/Presentation/Results/SuccessEnvelopeResult.cs`
- Create: `src/Common/Presentation/Results/ProblemEnvelopeResult.cs`
- Create: `src/Common/Presentation/Results/ApiResponse.cs`

**Step 1: Create SuccessEnvelopeResult**

```csharp
using Microsoft.AspNetCore.Http;

namespace Rtl.Core.Presentation.Results;

/// <summary>
/// IResult that writes a success envelope. Supports 200 OK and 201 Created.
/// </summary>
internal sealed class SuccessEnvelopeResult<T>(int statusCode, T? data, string? location = null) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (location is not null)
            httpContext.Response.Headers.Location = location;

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ApiEnvelope<T>
        {
            IsSuccess = true,
            Data = data,
            ProblemDetails = null
        }, httpContext.RequestAborted);
    }
}
```

**Step 2: Create ProblemEnvelopeResult**

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Rtl.Core.Domain.Results;

namespace Rtl.Core.Presentation.Results;

/// <summary>
/// IResult that writes a failure envelope with RFC 9457 ProblemDetails.
/// Resolves requestId, traceId, and instance from HttpContext at execution time.
/// </summary>
internal sealed class ProblemEnvelopeResult(Error error) : IResult
{
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var statusCode = GetStatusCode(error.Type);
        httpContext.Response.StatusCode = statusCode;

        var errors = error is ValidationError ve
            ? ve.Errors.Select(e => new ApiErrorDetail { Code = e.Code, Description = e.Description }).ToList()
            : [new ApiErrorDetail { Code = error.Code, Description = error.Description }];

        await httpContext.Response.WriteAsJsonAsync(new ApiEnvelope<object>
        {
            IsSuccess = false,
            Data = default,
            ProblemDetails = new ApiProblemEnvelope
            {
                Type = GetTypeUri(error.Type),
                Title = GetTitle(error.Type),
                Status = statusCode,
                Instance = httpContext.Request.Path,
                RequestId = httpContext.TraceIdentifier,
                TraceId = Activity.Current?.Id ?? string.Empty,
                Errors = errors
            }
        }, httpContext.RequestAborted);
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Problem => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Bad Request",
        ErrorType.Problem => "Bad Request",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        _ => "Server Failure"
    };

    private static string GetTypeUri(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };
}
```

**Step 3: Create ApiResponse static helper**

```csharp
using Microsoft.AspNetCore.Http;
using Rtl.Core.Domain.Results;

namespace Rtl.Core.Presentation.Results;

/// <summary>
/// Static factory for creating envelope-wrapped API responses.
/// Drop-in replacement for ApiResults — supports method-group syntax in Result.Match().
/// </summary>
public static class ApiResponse
{
    /// <summary>200 OK with data.</summary>
    public static IResult Ok<T>(T data) =>
        new SuccessEnvelopeResult<T>(StatusCodes.Status200OK, data);

    /// <summary>201 Created with data and Location header.</summary>
    public static IResult Created<T>(string location, T data) =>
        new SuccessEnvelopeResult<T>(StatusCodes.Status201Created, data, location);

    /// <summary>200 OK with null data (replaces 204 NoContent).</summary>
    public static IResult Success() =>
        new SuccessEnvelopeResult<object>(StatusCodes.Status200OK, null);

    /// <summary>Error response from a domain Error. Use as method group: ApiResponse.Problem</summary>
    public static IResult Problem(Error error) =>
        new ProblemEnvelopeResult(error);

    /// <summary>Error response from a failed Result.</summary>
    public static IResult Problem(Result result) =>
        Problem(result.Error);
}
```

**Step 4: Build**

Run: `dotnet build src/Common/Presentation/Rtl.Core.Presentation.csproj`
Expected: Success, 0 errors

**Step 5: Commit**

```
feat: add ApiResponse helpers and IResult envelope implementations
```

---

## Task 3: Write Unit Tests for ApiResponse

**Files:**
- Modify: `src/Common/test/Presentation.Tests/Results/ApiResultsTests.cs` (keep existing tests)
- Create: `src/Common/test/Presentation.Tests/Results/ApiResponseTests.cs`

**Step 1: Write tests**

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Rtl.Core.Domain.Results;
using Rtl.Core.Presentation.Results;
using Xunit;

namespace Rtl.Core.Presentation.Tests.Results;

public class ApiResponseTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task Ok_ReturnsSuccessEnvelopeWith200()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var result = ApiResponse.Ok(new { Name = "Test" });

        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(httpContext.Response.Body);
        Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("Test", json.RootElement.GetProperty("data").GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("problemDetails").ValueKind);
    }

    [Fact]
    public async Task Created_ReturnsSuccessEnvelopeWith201AndLocation()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var result = ApiResponse.Created("/api/v1/sales/123", new { Id = Guid.Empty });

        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status201Created, httpContext.Response.StatusCode);
        Assert.Equal("/api/v1/sales/123", httpContext.Response.Headers.Location.ToString());

        httpContext.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(httpContext.Response.Body);
        Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.NotEqual(JsonValueKind.Null, json.RootElement.GetProperty("data").ValueKind);
    }

    [Fact]
    public async Task Success_ReturnsEnvelopeWithNullData()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var result = ApiResponse.Success();

        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(httpContext.Response.Body);
        Assert.True(json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("data").ValueKind);
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("problemDetails").ValueKind);
    }

    [Fact]
    public async Task Problem_WithNotFoundError_Returns404Envelope()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Path = "/api/v1/sales/abc";

        var error = Error.NotFound("Sale.NotFound", "Sale not found");
        var result = ApiResponse.Problem(error);

        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(httpContext.Response.Body);
        var root = json.RootElement;

        Assert.False(root.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("data").ValueKind);

        var pd = root.GetProperty("problemDetails");
        Assert.Equal(404, pd.GetProperty("status").GetInt32());
        Assert.Equal("Not Found", pd.GetProperty("title").GetString());
        Assert.Equal("/api/v1/sales/abc", pd.GetProperty("instance").GetString());

        var errors = pd.GetProperty("errors");
        Assert.Equal(1, errors.GetArrayLength());
        Assert.Equal("Sale.NotFound", errors[0].GetProperty("code").GetString());
        Assert.Equal("Sale not found", errors[0].GetProperty("description").GetString());
    }

    [Fact]
    public async Task Problem_WithValidationError_Returns400WithMultipleErrors()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var validationError = new ValidationError([
            Error.Validation("Name", "Name is required"),
            Error.Validation("Amount", "Amount must be positive")
        ]);

        var result = ApiResponse.Problem(validationError);

        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);

        httpContext.Response.Body.Position = 0;
        var json = await JsonDocument.ParseAsync(httpContext.Response.Body);
        var errors = json.RootElement.GetProperty("problemDetails").GetProperty("errors");
        Assert.Equal(2, errors.GetArrayLength());
    }

    [Fact]
    public async Task Problem_FromFailedResult_Returns500Envelope()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var failedResult = Result.Failure(Error.Failure("Server.Error", "Something broke"));
        var result = ApiResponse.Problem(failedResult);

        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }
}
```

**Step 2: Run tests**

Run: `dotnet test src/Common/test/Presentation.Tests/ --filter "FullyQualifiedName~ApiResponseTests"`
Expected: All 6 pass

**Step 3: Commit**

```
test: add unit tests for ApiResponse envelope helpers
```

---

## Task 4: Update GlobalExceptionHandler

**Files:**
- Modify: `src/Common/Presentation/GlobalExceptionHandler.cs`

**Step 1: Update to return envelope**

Replace the full class body:

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Rtl.Core.Presentation.Results;

namespace Rtl.Core.Presentation;

/// <summary>
/// Global exception handler that returns the API envelope with RFC 9457 ProblemDetails.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ApiEnvelope<object>
        {
            IsSuccess = false,
            Data = default,
            ProblemDetails = new ApiProblemEnvelope
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Server Failure",
                Status = StatusCodes.Status500InternalServerError,
                Instance = httpContext.Request.Path,
                RequestId = httpContext.TraceIdentifier,
                TraceId = Activity.Current?.Id ?? string.Empty,
                Errors = [new ApiErrorDetail { Code = "Server.InternalError", Description = "An unexpected error occurred." }]
            }
        }, cancellationToken);

        return true;
    }
}
```

**Step 2: Build**

Run: `dotnet build src/Common/Presentation/Rtl.Core.Presentation.csproj`
Expected: Success — the `Microsoft.AspNetCore.Mvc` using for `ProblemDetails` is no longer needed.

**Step 3: Commit**

```
refactor: update GlobalExceptionHandler to return API envelope
```

---

## Task 5: Update FeatureFlagEndpointFilter

**Files:**
- Modify: `src/Common/Presentation/FeatureManagement/FeatureFlagEndpointFilter.cs`

**Step 1: Replace `Results.Problem(...)` with envelope**

Change the `!isEnabled` block from:

```csharp
return Microsoft.AspNetCore.Http.Results.Problem(
    title: "Not Found",
    detail: "The requested resource was not found.",
    type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    statusCode: StatusCodes.Status404NotFound);
```

To:

```csharp
return ApiResponse.Problem(
    Rtl.Core.Domain.Results.Error.NotFound(
        "Feature.Disabled", "The requested resource was not found."));
```

Add using: `using Rtl.Core.Presentation.Results;`

**Step 2: Build**

Run: `dotnet build src/Common/Presentation/Rtl.Core.Presentation.csproj`
Expected: Success

**Step 3: Commit**

```
refactor: update FeatureFlagEndpointFilter to return API envelope
```

---

## Task 6: Update Sales and DeliveryAddress Endpoints

**Files to modify (5 endpoints):**
- `src/Modules/Sales/Presentation/Endpoints/V1/Sales/CreateSaleEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Sales/GetSaleByIdEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/DeliveryAddress/CreateDeliveryAddressEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/DeliveryAddress/UpdateDeliveryAddressEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/DeliveryAddress/GetDeliveryAddressEndpoint.cs`

**Pattern for every endpoint — three changes:**

1. Replace `using Rtl.Core.Presentation.Results;` (already there, reuse it — `ApiResponse` is in the same namespace)
2. Update the `result.Match(...)` call
3. Update `.Produces<T>(...)` to `.Produces<ApiEnvelope<T>>(...)` for success status codes

**Specific changes per endpoint:**

**CreateSaleEndpoint.cs:**
```csharp
// .Produces line
.Produces<ApiEnvelope<CreateSaleResponse>>(StatusCodes.Status201Created)

// Match
return result.Match(
    r => ApiResponse.Created($"/api/v1/sales/{r.PublicId}", new CreateSaleResponse(r.PublicId, r.SaleNumber)),
    ApiResponse.Problem);
```

**GetSaleByIdEndpoint.cs:**
```csharp
.Produces<ApiEnvelope<GetSaleByIdResponse>>(StatusCodes.Status200OK)

return result.Match(
    r => ApiResponse.Ok(r),  // or whatever the current Ok mapping is
    ApiResponse.Problem);
```

**CreateDeliveryAddressEndpoint.cs:**
```csharp
.Produces<ApiEnvelope<CreateDeliveryAddressResponse>>(StatusCodes.Status201Created)

return result.Match(
    r => ApiResponse.Created($"/api/v1/sales/{publicSaleId}/delivery-address", new CreateDeliveryAddressResponse(r.PublicId)),
    ApiResponse.Problem);
```

**UpdateDeliveryAddressEndpoint.cs:**
```csharp
// Change from .Produces(StatusCodes.Status204NoContent) to:
.Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)

// Change from Results.NoContent() to:
return result.Match(
    () => ApiResponse.Success(),
    ApiResponse.Problem);
```

**GetDeliveryAddressEndpoint.cs:**
```csharp
.Produces<ApiEnvelope<DeliveryAddressResponse>>(StatusCodes.Status200OK)

return result.Match(
    r => ApiResponse.Ok(r),
    ApiResponse.Problem);
```

**Step: Build**

Run: `dotnet build src/Modules/Sales/Presentation/Modules.Sales.Presentation.csproj`
Expected: Success

**Commit:**

```
refactor: update Sales and DeliveryAddress endpoints to return API envelope
```

---

## Task 7: Update Package CRUD Endpoints

**Files to modify (6 endpoints):**
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/CreatePackageEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/GetPackageByIdEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/GetPackagesBySaleEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/DeletePackageEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/SetPackageAsPrimaryEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/UpdatePackageNameEndpoint.cs`

**Same three-change pattern. Notable differences:**

**CreatePackageEndpoint.cs:**
```csharp
.Produces<ApiEnvelope<CreatePackageResponse>>(StatusCodes.Status201Created)

r => ApiResponse.Created($"/api/v1/sales/{publicSaleId}/packages/{r.PublicId}", new CreatePackageResponse(r.PublicId)),
```

**GetPackageByIdEndpoint.cs:**
```csharp
.Produces<ApiEnvelope<PackageDetailResponse>>(StatusCodes.Status200OK)

r => ApiResponse.Ok(r),
```

**GetPackagesBySaleEndpoint.cs:**
```csharp
.Produces<ApiEnvelope<IReadOnlyCollection<PackageSummaryResponse>>>(StatusCodes.Status200OK)

ApiResponse.Ok,   // method group still works for identity mapping
```

**DeletePackageEndpoint.cs, SetPackageAsPrimaryEndpoint.cs, UpdatePackageNameEndpoint.cs:**
```csharp
// Change .Produces(StatusCodes.Status204NoContent) to:
.Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)

// Change Results.NoContent() to:
() => ApiResponse.Success(),
```

**Build and commit:**

```
refactor: update Package CRUD endpoints to return API envelope
```

---

## Task 8: Update Package Update Endpoints (10 endpoints)

**Files to modify:**
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/Home/UpdatePackageHomeEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/Land/UpdatePackageLandEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/Insurance/UpdatePackageInsuranceEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/Warranty/UpdatePackageWarrantyEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/DownPayment/UpdatePackageDownPaymentEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/Concessions/UpdatePackageConcessionsEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/SalesTeam/UpdatePackageSalesTeamEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/TradeIns/UpdatePackageTradeInsEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Packages/ProjectCosts/UpdatePackageProjectCostsEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Tax/UpdatePackageTaxEndpoint.cs`

**All 10 share the same pattern — they return `PackageUpdatedResponse`:**

```csharp
// .Produces line (all 10 endpoints)
.Produces<ApiEnvelope<PackageUpdatedResponse>>(StatusCodes.Status200OK)

// Match (all 10 endpoints)
return result.Match(
    r => ApiResponse.Ok(new PackageUpdatedResponse(
        r.GrossProfit,
        r.CommissionableGrossProfit,
        r.MustRecalculateTaxes)),
    ApiResponse.Problem);
```

**Build and commit:**

```
refactor: update 10 Package update endpoints to return API envelope
```

---

## Task 9: Update Pricing Endpoints (5 endpoints)

**Files to modify:**
- `src/Modules/Sales/Presentation/Endpoints/V1/Pricing/GetRetailPriceEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Pricing/GetOptionTotalsEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Pricing/GetWheelsAndAxlesPriceEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Pricing/GetWheelsAndAxlesPriceByStockEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Pricing/GetHomeMultipliersEndpoint.cs`

**Same pattern — each returns its own response type:**

```csharp
.Produces<ApiEnvelope<RetailPriceResponse>>(StatusCodes.Status200OK)
// etc. per endpoint

r => ApiResponse.Ok(r),  // or ApiResponse.Ok(new ResponseType(...))
ApiResponse.Problem
```

**Build and commit:**

```
refactor: update Pricing endpoints to return API envelope
```

---

## Task 10: Update Tax, Commission, and ProjectCost Reference Endpoints

**Files to modify:**
- `src/Modules/Sales/Presentation/Endpoints/V1/Tax/GetTaxExemptionsEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Tax/GetTaxQuestionsEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Tax/CalculateTaxesEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/Commission/CalculateCommissionEndpoint.cs`
- `src/Modules/Sales/Presentation/Endpoints/V1/ProjectCosts/GetProjectCostCategoriesEndpoint.cs`

**Same pattern:**

```csharp
.Produces<ApiEnvelope<IReadOnlyCollection<TaxExemptionResponse>>>(StatusCodes.Status200OK)
// etc. per endpoint

r => ApiResponse.Ok(r),
ApiResponse.Problem
```

**Build and commit:**

```
refactor: update Tax, Commission, and ProjectCost endpoints to return API envelope
```

---

## Task 11: Update InsuranceQuoteEndpoint (special case)

**Files:**
- Modify: `src/Modules/Sales/Presentation/Endpoints/V1/Insurance/InsuranceQuoteEndpoint.cs`

This endpoint is unique — it dispatches by query param and has multiple handlers with direct `Results.Problem()` calls.

**Step 1: Update .Produces metadata**

```csharp
.Produces<ApiEnvelope<HomeFirstQuoteResponse>>(StatusCodes.Status200OK)
.Produces<ApiEnvelope<object>>(StatusCodes.Status201Created)
```

**Step 2: Update the type switch (direct Results.Problem calls)**

```csharp
// Before
"print" => Task.FromResult(Results.Ok(new PrintInsuranceQuoteResponse(...))),
_ => Task.FromResult(Results.Problem(
    $"Unknown insurance quote type: '{type}'.", statusCode: StatusCodes.Status400BadRequest))

// After
"print" => Task.FromResult(ApiResponse.Ok(new PrintInsuranceQuoteResponse(...))),
_ => Task.FromResult(ApiResponse.Problem(
    Error.Validation("Insurance.InvalidType", $"Unknown insurance quote type: '{type}'. Valid values: home-first, warranty, outside, print.")))
```

**Step 3: Update HomeFirst handler body-null check**

```csharp
// Before
return Results.Problem(detail: "Request body is required for HomeFirst insurance quote.", statusCode: StatusCodes.Status400BadRequest);

// After
return ApiResponse.Problem(Error.Validation("Insurance.MissingBody", "Request body is required for HomeFirst insurance quote."));
```

**Step 4: Update Outside handler body-null check (same pattern)**

**Step 5: Update all three Match calls**

```csharp
// HomeFirst
return result.Match(
    r => ApiResponse.Ok(new HomeFirstQuoteResponse(...)),
    ApiResponse.Problem);

// Warranty
return result.Match(
    r => ApiResponse.Ok(new WarrantyQuoteResponse(...)),
    ApiResponse.Problem);

// Outside — was Results.Created(), now ApiResponse.Success()
return result.Match(
    () => ApiResponse.Success(),
    ApiResponse.Problem);
```

**Step 6: Add using for Error type**

```csharp
using Rtl.Core.Domain.Results;
```

**Build and commit:**

```
refactor: update InsuranceQuoteEndpoint to return API envelope
```

---

## Task 12: Build, Test, and Verify

**Step 1: Full build**

Run: `dotnet build`
Expected: 0 errors, 0 warnings about nullability

**Step 2: Run all tests**

Run: `dotnet test`
Expected: All Sales domain + application tests pass (385+). ApiResponse tests pass (6).

**Step 3: Verify via live API call**

Drop and reseed the database, then hit the API:

```bash
curl -s http://localhost:5000/api/v1/sales/c5331d22-bb8e-2c4f-bc06-589c0aad842c/packages | python3 -m json.tool
```

Expected output:
```json
{
    "isSuccess": true,
    "data": [
        {
            "id": "ab743b1f-bba5-574d-83f8-3c0dd1424ab3",
            "name": "Primary",
            "ranking": 1,
            "isPrimaryPackage": true,
            "status": "Draft",
            "grossProfit": 0,
            "commissionableGrossProfit": 0,
            "mustRecalculateTaxes": true
        }
    ],
    "problemDetails": null
}
```

**Step 4: Verify error response**

```bash
curl -s http://localhost:5000/api/v1/sales/00000000-0000-0000-0000-000000000000/packages | python3 -m json.tool
```

Expected: 404 with envelope:
```json
{
    "isSuccess": false,
    "data": null,
    "problemDetails": {
        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        "title": "Not Found",
        "status": 404,
        "instance": "/api/v1/sales/00000000-0000-0000-0000-000000000000/packages",
        "requestId": "...",
        "traceId": "...",
        "errors": [
            {
                "code": "Sale.NotFound",
                "description": "..."
            }
        ]
    }
}
```

**Step 5: Final commit**

```
feat: complete API response envelope — all 32 endpoints return uniform { isSuccess, data, problemDetails }
```

---

## Summary

| Task | Files | What |
|------|-------|------|
| 1 | 1 create | Envelope types |
| 2 | 3 create | IResult impls + ApiResponse helper |
| 3 | 1 create | Unit tests (6 tests) |
| 4 | 1 modify | GlobalExceptionHandler |
| 5 | 1 modify | FeatureFlagEndpointFilter |
| 6 | 5 modify | Sales + DeliveryAddress endpoints |
| 7 | 6 modify | Package CRUD endpoints |
| 8 | 10 modify | Package update endpoints |
| 9 | 5 modify | Pricing endpoints |
| 10 | 5 modify | Tax + Commission + ProjectCost endpoints |
| 11 | 1 modify | InsuranceQuoteEndpoint (special) |
| 12 | 0 | Build, test, verify |

**Total: 4 new files, 29 modified files, 12 tasks**
