using Microsoft.AspNetCore.Http;
using Rtl.Core.Application.Auditing;
using System.Diagnostics;

namespace Rtl.Core.Infrastructure.Auditing;

/// <summary>
/// Captures audit context from HTTP request and distributed tracing.
/// </summary>
internal sealed class AuditContext(IHttpContextAccessor httpContextAccessor) : IAuditContext
{
    public string? UserName =>
        httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.TraceIdentifier;

    public string? TraceId =>
        Activity.Current?.TraceId.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
