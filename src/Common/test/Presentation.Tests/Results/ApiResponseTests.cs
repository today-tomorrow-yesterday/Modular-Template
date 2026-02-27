using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Rtl.Core.Domain.Results;
using Rtl.Core.Presentation.Results;
using Xunit;

namespace Rtl.Core.Presentation.Tests.Results;

public class ApiResponseTests
{
    [Fact]
    public async Task Ok_ReturnsSuccessEnvelopeWith200()
    {
        var data = new TestDto("hello");
        var result = ApiResponse.Ok(data);
        var (statusCode, envelope) = await ExecuteAsync<TestDto>(result);

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.True(envelope.IsSuccess);
        Assert.NotNull(envelope.Data);
        Assert.Equal("hello", envelope.Data!.Value);
        Assert.Null(envelope.ProblemDetails);
    }

    [Fact]
    public async Task Created_ReturnsSuccessEnvelopeWith201AndLocationHeader()
    {
        var data = new TestDto("created");
        var result = ApiResponse.Created("/items/1", data);
        var httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        await result.ExecuteAsync(httpContext);

        Assert.Equal(StatusCodes.Status201Created, httpContext.Response.StatusCode);
        Assert.Equal("/items/1", httpContext.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task Success_ReturnsSuccessEnvelopeWith200AndNullData()
    {
        var result = ApiResponse.Success();
        var (statusCode, envelope) = await ExecuteAsync<object>(result);

        Assert.Equal(StatusCodes.Status200OK, statusCode);
        Assert.True(envelope.IsSuccess);
        Assert.Null(envelope.Data);
        Assert.Null(envelope.ProblemDetails);
    }

    [Fact]
    public async Task Problem_WithNotFoundError_Returns404WithProblemDetails()
    {
        var error = Error.NotFound("Entity.NotFound", "Not found");
        var result = ApiResponse.Problem(error);
        var (statusCode, envelope) = await ExecuteAsync<object>(result);

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.False(envelope.IsSuccess);
        Assert.Null(envelope.Data);
        Assert.NotNull(envelope.ProblemDetails);
        Assert.Equal(404, envelope.ProblemDetails!.Status);
        Assert.Single(envelope.ProblemDetails.Errors);
        Assert.Equal("Entity.NotFound", envelope.ProblemDetails.Errors[0].Code);
        Assert.Equal("Not found", envelope.ProblemDetails.Errors[0].Description);
    }

    [Fact]
    public async Task Problem_WithValidationError_Returns400()
    {
        var error = Error.Validation("Field.Invalid", "Invalid");
        var result = ApiResponse.Problem(error);
        var (statusCode, envelope) = await ExecuteAsync<object>(result);

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.False(envelope.IsSuccess);
        Assert.Equal(400, envelope.ProblemDetails!.Status);
    }

    [Fact]
    public async Task Problem_WithConflictError_Returns409()
    {
        var error = Error.Conflict("Entity.Conflict", "Already exists");
        var result = ApiResponse.Problem(error);
        var (statusCode, envelope) = await ExecuteAsync<object>(result);

        Assert.Equal(StatusCodes.Status409Conflict, statusCode);
        Assert.Equal(409, envelope.ProblemDetails!.Status);
    }

    [Fact]
    public async Task Problem_WithFailureError_Returns500()
    {
        var error = Error.Failure("Server.Error", "Error");
        var result = ApiResponse.Problem(error);
        var (statusCode, envelope) = await ExecuteAsync<object>(result);

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.Equal(500, envelope.ProblemDetails!.Status);
    }

    [Fact]
    public void Problem_WithSuccessResult_ThrowsInvalidOperationException()
    {
        var result = Result.Success();

        Assert.Throws<InvalidOperationException>(() => ApiResponse.Problem(result));
    }

    [Fact]
    public async Task Problem_IncludesRequestIdAndInstance()
    {
        var error = Error.NotFound("Test", "Test");
        var result = ApiResponse.Problem(error);
        var httpContext = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
            TraceIdentifier = "test-request-id"
        };
        httpContext.Request.Path = "/api/v1/test";
        httpContext.Request.QueryString = new QueryString("?type=outside");

        await result.ExecuteAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        var envelope = await JsonSerializer.DeserializeAsync<ApiEnvelope<object>>(httpContext.Response.Body, DeserializeOptions);

        Assert.NotNull(envelope?.ProblemDetails);
        Assert.Equal("test-request-id", envelope!.ProblemDetails!.RequestId);
        Assert.Equal("/api/v1/test?type=outside", envelope.ProblemDetails.Instance);
    }

    [Fact]
    public async Task Problem_ProblemDetails_HasCorrectTypeUri()
    {
        var error = Error.NotFound("Test", "Test");
        var result = ApiResponse.Problem(error);
        var (_, envelope) = await ExecuteAsync<object>(result);

        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.4", envelope.ProblemDetails!.Type);
        Assert.Equal("Not Found", envelope.ProblemDetails.Title);
    }

    private static readonly JsonSerializerOptions DeserializeOptions = new(JsonSerializerDefaults.Web);

    private static async Task<(int StatusCode, ApiEnvelope<T> Envelope)> ExecuteAsync<T>(IResult result)
    {
        var httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        await result.ExecuteAsync(httpContext);

        httpContext.Response.Body.Position = 0;
        var envelope = await JsonSerializer.DeserializeAsync<ApiEnvelope<T>>(httpContext.Response.Body, DeserializeOptions);

        return (httpContext.Response.StatusCode, envelope!);
    }

    private sealed record TestDto(string Value);
}
