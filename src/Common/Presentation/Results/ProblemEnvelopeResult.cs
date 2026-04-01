using Microsoft.AspNetCore.Http;
using ModularTemplate.Domain.Results;
using System.Diagnostics;

namespace ModularTemplate.Presentation.Results;

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
                Instance = httpContext.Request.Path + httpContext.Request.QueryString,
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
