using System.Net;
using Xunit;

namespace Rtl.Core.IntegrationTests;

// Static helpers for asserting HTTP status codes with descriptive failure messages.
// Includes the response body in the failure message so you know WHY it failed,
// not just that the status code was wrong.
// Usage: await HttpAssert.IsCreatedAsync(response);
public static class HttpAssert
{
    public static Task IsOkAsync(HttpResponseMessage response)
        => AssertStatusAsync(HttpStatusCode.OK, response);

    public static Task IsCreatedAsync(HttpResponseMessage response)
        => AssertStatusAsync(HttpStatusCode.Created, response);

    public static Task IsNoContentAsync(HttpResponseMessage response)
        => AssertStatusAsync(HttpStatusCode.NoContent, response);

    public static Task IsBadRequestAsync(HttpResponseMessage response)
        => AssertStatusAsync(HttpStatusCode.BadRequest, response);

    public static Task IsNotFoundAsync(HttpResponseMessage response)
        => AssertStatusAsync(HttpStatusCode.NotFound, response);

    public static Task IsConflictAsync(HttpResponseMessage response)
        => AssertStatusAsync(HttpStatusCode.Conflict, response);

    public static Task IsUnauthorizedAsync(HttpResponseMessage response)
        => AssertStatusAsync(HttpStatusCode.Unauthorized, response);

    public static Task IsForbiddenAsync(HttpResponseMessage response)
        => AssertStatusAsync(HttpStatusCode.Forbidden, response);

    private static async Task AssertStatusAsync(HttpStatusCode expected, HttpResponseMessage response)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            Assert.Fail(
                $"Expected {(int)expected} {expected}, got {(int)response.StatusCode} {response.StatusCode}.\n" +
                $"Response: {body}");
        }
    }
}
