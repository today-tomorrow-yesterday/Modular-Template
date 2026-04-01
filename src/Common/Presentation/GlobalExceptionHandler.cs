using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModularTemplate.Presentation.Results;
using System.Diagnostics;

namespace ModularTemplate.Presentation;

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
                Instance = httpContext.Request.Path + httpContext.Request.QueryString,
                RequestId = httpContext.TraceIdentifier,
                TraceId = Activity.Current?.Id ?? string.Empty,
                Errors = [new ApiErrorDetail { Code = "Server.InternalError", Description = "An unexpected error occurred." }]
            }
        }, cancellationToken);

        return true;
    }
}
