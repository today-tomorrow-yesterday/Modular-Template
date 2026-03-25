using System.Net;
using System.Net.Http.Json;
using Modules.Sales.Application.Packages.GetPackageById;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.EndpointTests.Abstractions;

// Generic HTTP helper extension methods for integration tests.
// Domain-specific setup helpers live on SalesEndpointTestBase.
public static class SalesHttpHelpers
{
    public static async Task<T?> GetAsync<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"GET {url} failed: {response.StatusCode} — {errorBody}");
        }
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<T?> PostAsync<T>(this HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"POST {url} failed: {response.StatusCode} — {errorBody}");
        }
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<HttpResponseMessage> PutAndAssertOkAsync(
        this HttpClient client, string url, object body)
    {
        var response = await client.PutAsJsonAsync(url, body);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var raw = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"PUT {url} failed: {response.StatusCode} — {raw}");
        }
        return response;
    }

    public static async Task<T?> PutAsync<T>(this HttpClient client, string url, object body)
    {
        var response = await client.PutAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"PUT {url} failed: {response.StatusCode} — {errorBody}");
        }
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<PackageDetailResponse> GetPackageAsync(
        this HttpClient client, Guid packageId)
    {
        var body = await client.GetAsync<ApiEnvelope<PackageDetailResponse>>(
            $"/api/v1/packages/{packageId}");
        return body!.Data!;
    }
}
