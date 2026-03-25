using System.Net;
using Xunit;

namespace Rtl.Core.IntegrationTests;

// Static helpers for asserting HTTP status codes with descriptive failure messages.
// Usage: HttpAssert.IsCreated(response);
public static class HttpAssert
{
    public static void IsOk(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    public static void IsCreated(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Created, response.StatusCode);

    public static void IsNoContent(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    public static void IsBadRequest(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

    public static void IsNotFound(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    public static void IsConflict(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

    public static void IsUnauthorized(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

    public static void IsForbidden(HttpResponseMessage response)
        => Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
