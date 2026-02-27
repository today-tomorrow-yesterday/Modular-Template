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
