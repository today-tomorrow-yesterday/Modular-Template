using System.Net;

namespace Rtl.Core.Infrastructure.Tests.ISeries;

/// <summary>
/// Hand-rolled fake for HttpMessageHandler. Captures request details and returns a configured response.
/// </summary>
internal sealed class FakeMessageHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }
    public HttpResponseMessage ResponseToReturn { get; set; } = new(HttpStatusCode.OK);
    public int CallCount { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        LastRequest = request;

        if (request.Content is not null)
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

        return ResponseToReturn;
    }
}
